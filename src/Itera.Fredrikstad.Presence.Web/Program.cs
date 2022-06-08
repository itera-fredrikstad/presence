using System.Net;
using Ardalis.SmartEnum.SystemTextJson;
using Itera.Fredrikstad.Presence.Api;
using Itera.Fredrikstad.Presence.Core;
using Itera.Fredrikstad.Presence.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();

builder.Services.AddEndpointsProvidesMetadataApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JsonOptions>(opts =>
{
    opts.SerializerOptions.WriteIndented = true;
    opts.SerializerOptions.Converters.Add(new SmartEnumNameConverter<DayType, int>());
});

builder.Services.AddDbContext<Db>((provider, opt) => opt.UseSqlServer(provider.GetService<IConfiguration>().GetConnectionString("Sql"), sql => sql.EnableRetryOnFailure()));


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
    var context = services.GetRequiredService<Db>();
    context.Database.EnsureCreated();
}

app.UseCors(opts =>
{
    opts.AllowAnyOrigin();
    opts.AllowAnyMethod();
    opts.AllowAnyHeader();
});

app.UseCookiePolicy();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapApi();
app.MapFallbackToFile("index.html");

app.Run();
