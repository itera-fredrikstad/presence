using Ardalis.SmartEnum.SystemTextJson;
using Itera.Fredrikstad.Presence.Core;
using Itera.Fredrikstad.Presence.Infrastructure;
using Itera.Fredrikstad.Presence.Web;
using Itera.Fredrikstad.Presence.Web.Api.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System.Net.Http.Headers;
using WebApplication = Microsoft.AspNetCore.Builder.WebApplication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddSpaYarp();

builder.Services.AddCors();

builder.Services.AddEndpointsProvidesMetadataApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JsonOptions>(opts =>
{
    opts.SerializerOptions.WriteIndented = true;
    opts.SerializerOptions.Converters.Add(new SmartEnumNameConverter<DayType, int>());
});

builder.Services.AddDbContext(builder.Configuration.GetConnectionString("Sql"));

builder.Services.Configure<CookiePolicyOptions>(options => { options.Secure = CookieSecurePolicy.Always; });

var scopes = builder.Configuration["AzureAd:Scopes"];

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.Bind("AzureAd", options);

        options.Events.OnTokenValidated = async context =>
        {
            var tokenAcquisition = context.HttpContext.RequestServices
                .GetRequiredService<ITokenAcquisition>();

            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(async (request) =>
                {
                    var token = await tokenAcquisition
                        .GetAccessTokenForUserAsync(scopes.Split(' '), user: context.Principal);
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                })
            );

            // Get user information from Graph
            var user = await graphClient.Me.Request()
                .Select(u => new
                {
                    u.DisplayName,
                    u.UserPrincipalName,
                    u.OfficeLocation
                })
                .GetAsync();

            context.Principal?.AddUserGraphInfo(user);

            // Get the user's photo
            // If the user doesn't have a photo, this throws
            try
            {
                var photo = await graphClient.Me
                    .Photos["48x48"]
                    .Content
                    .Request()
                    .GetAsync();

                context.Principal?.AddUserGraphPhoto(photo);
            }
            catch (ServiceException ex)
            {
                if (ex.IsMatch("ErrorItemNotFound") ||
                    ex.IsMatch("ConsumerPhotoIsNotSupported"))
                {
                    context.Principal?.AddUserGraphPhoto(null);
                }
                else
                {
                    throw;
                }
            }
        };

        options.Events.OnRedirectToIdentityProvider = context => 
        {
            if(context.Request.Path.StartsWithSegments("/api"))
            {
                context.HandleResponse();
                context.Response.Clear();
                context.Response.StatusCode = 401;
            }

            return Task.CompletedTask;
        };
    })
    // </AddSignInSnippet>
    // Add ability to call web API (Graph)
    // and get access tokens
    .EnableTokenAcquisitionToCallDownstreamApi(options => { builder.Configuration.Bind("AzureAd", options); },
        scopes.Split(' '))
    // <AddGraphClientSnippet>
    // Add a GraphServiceClient via dependency injection
    .AddMicrosoftGraph(options => { options.Scopes = scopes; })
    // </AddGraphClientSnippet>
    // Use in-memory token cache
    // See https://github.com/AzureAD/microsoft-identity-web/wiki/token-cache-serialization
    .AddInMemoryTokenCaches();

builder.Services.AddTransient<IDayAtWorkRepository, DayAtWorkSqlRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<PresenceDbContext>();
    context.Database.EnsureCreated();
}

app.UseCors(opts =>
{
    opts.AllowAnyOrigin();
    opts.AllowAnyMethod();
    opts.AllowAnyHeader();
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseCookiePolicy();

app.UseAuthentication();
app.UseRouting();

app.Use(async (context, next) =>
{
    var endpointMetadata = context.Features.Get<IEndpointFeature>()?.Endpoint?.Metadata;
    
    if (context.User.Identity is { IsAuthenticated: true })
    {
        await next();
        return;
    }

    if(endpointMetadata?.Any(m => m is AllowAnonymousAttribute) ?? false) 
    {
        await next();
        return;
    }

    await context.ChallengeAsync();
});

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSpaYarp();
}

app.MapApi();
app.MapFallbackToFile("index.html");

app.Run();