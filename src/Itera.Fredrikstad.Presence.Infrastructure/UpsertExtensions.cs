using Microsoft.EntityFrameworkCore;

namespace Itera.Fredrikstad.Presence.Infrastructure;

public static class DbContextExtensions
{
    public static async Task AddOrUpdate<TEntity>(this DbContext db, TEntity entity, Func<TEntity, object[]> keySelector) where TEntity : class
    {
        var existing = await db.Set<TEntity>().FindAsync(keySelector(entity));

        if (existing != null)
        {
            db.Entry(existing).State = EntityState.Detached;
            db.Attach(entity).State = EntityState.Modified;
        }
        else
        {
            await db.AddAsync(entity);
        }
    }
}
