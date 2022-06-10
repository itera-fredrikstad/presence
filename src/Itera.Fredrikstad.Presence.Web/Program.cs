using System.Net.Http.Headers;
using Ardalis.SmartEnum.SystemTextJson;
using Itera.Fredrikstad.Presence.Core;
using Itera.Fredrikstad.Presence.Infrastructure;
using Itera.Fredrikstad.Presence.Web.Api.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using WebApplication = Microsoft.AspNetCore.Builder.WebApplication;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.Secure = CookieSecurePolicy.Always;
});
var scopes = builder.Configuration["AzureAd:Scopes"];

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApp(builder.Configuration)
                // </AddSignInSnippet>
                // Add ability to call web API (Graph)
                // and get access tokens
                .EnableTokenAcquisitionToCallDownstreamApi(options =>
                {
                    builder.Configuration.Bind("AzureAd", options);
                }, scopes.Split(' '))
                // <AddGraphClientSnippet>
                // Add a GraphServiceClient via dependency injection
                .AddMicrosoftGraph(options =>
                {
                    options.Scopes = scopes;
                })
                // </AddGraphClientSnippet>
                // Use in-memory token cache
                // See https://github.com/AzureAD/microsoft-identity-web/wiki/token-cache-serialization
                .AddInMemoryTokenCaches();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser().Build();
});

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

app.UseCookiePolicy();

app.UseAuthentication();

app.Use(async (context, next) =>
{
    if (!(context.User.Identity?.IsAuthenticated ?? false) && !context.Request.Path.ToString().Equals("/api/daySummary"))
    {
        await context.ChallengeAsync();
    }
    else
    {
        await next();
    }
});

if (!app.Environment.IsDevelopment())
{
    app.UseSpaYarp();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSpaYarp();
}

app.MapApi();
app.MapFallbackToFile("index.html");

app.Run();
