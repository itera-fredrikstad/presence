using Ardalis.SmartEnum.SystemTextJson;
using Itera.Fredrikstad.Presence.Core;
using Itera.Fredrikstad.Presence.Infrastructure;
using Itera.Fredrikstad.Presence.Web.Api.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

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

builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration);

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

app.Use(async (context, next) => {
    if(!(context.User.Identity?.IsAuthenticated ?? false))
    {
        await context.ChallengeAsync();
    }
    else 
    {
        await next();
    }
});

app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

if (!app.Configuration.GetValue<bool>("FeatureManagement:DisableSpa"))
{
    app.UseSpaYarp();
}

app.MapApi();
app.MapFallbackToFile("index.html");

app.Run();
