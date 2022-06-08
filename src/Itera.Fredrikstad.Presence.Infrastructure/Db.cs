using Itera.Fredrikstad.Presence.Core;
using Microsoft.EntityFrameworkCore;
using SmartEnum.EFCore;

namespace Itera.Fredrikstad.Presence.Infrastructure;

public class Db : DbContext
{
    public DbSet<DayAtWork> DayAtWorks{ get; set; }

    public Db(DbContextOptions<Db> opts) : base(opts) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ConfigureSmartEnum();
        modelBuilder.Entity<DayAtWork>()
            .HasKey(d => new { d.UserId, d.Date });
    }
}
