using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Itera.Fredrikstad.Presence.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static void AddDbContext(this IServiceCollection services, string connectionString) =>
      services.AddDbContext<PresenceDbContext>(options =>
          options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
}