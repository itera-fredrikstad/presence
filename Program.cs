using Ardalis.SmartEnum.SystemTextJson;
using Itera.Fredrikstad.Presence;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();

builder.Services.AddEndpointsProvidesMetadataApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JsonOptions>(opts =>
{
    opts.SerializerOptions.WriteIndented = true;
    opts.SerializerOptions.Converters.Add(new SmartEnumNameConverter<DayType, int>());
});

builder.Services.AddDbContext<Db>((provider, opt) => opt.UseSqlServer(provider.GetService<IConfiguration>().GetConnectionString("Sql")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<Db>();
    context.Database.EnsureCreated();
}

app.UseCors(opts => {
    opts.AllowAnyOrigin();
    opts.AllowAnyMethod();
    opts.AllowAnyHeader();
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapApi();
app.MapFallbackToFile("index.html");

app.Run();
