using Itera.Fredrikstad.Presence.Core;
using Microsoft.EntityFrameworkCore;
using SmartEnum.EFCore;

namespace Itera.Fredrikstad.Presence.Infrastructure;

public class PresenceDbContext : DbContext
{
    public DbSet<DayAtWork> DayAtWorks{ get; set; }

    public PresenceDbContext(DbContextOptions<PresenceDbContext> opts) : base(opts) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ConfigureSmartEnum();
        modelBuilder.Entity<DayAtWork>()
            .HasKey(d => new { d.UserId, d.Date });
    }
}
