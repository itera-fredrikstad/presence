using System.Runtime.InteropServices.ComTypes;
using Ardalis.SmartEnum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApis.Extensions.Results;

namespace Itera.Fredrikstad.Presence;

public static class Api
{
    public static void MapApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("dayAtWork/{userId}", Get);
        app.MapPut("dayAtWork", Update);
    }

    private static async Task<Ok<List<DayAtWork>>> Get(string userId, [FromServices] Db db) => 
        Results.Extensions.Ok(await db.DayAtWorks.AsNoTracking().Where(d => d.UserId.Equals(userId) && d.Date >= DateTime.Today).ToListAsync());

    private static async Task<Ok> Update([FromBody]DayAtWork dayAtWork, [FromServices] Db db)
    {   
        await db.AddOrUpdate(dayAtWork, d => new object[] { d.UserId, d.Date });
        await db.SaveChangesAsync();
        
        return Results.Extensions.Ok();
    }
}

public record DayAtWork(
    string UserId,
    DateTime Date,
    DayType Type,
    string? Comment = null);

public static class UpsertExtensions
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

public class DayType : SmartEnum<DayType>
{
    public static DayType Full = new("FULL", 0);
    public static DayType FirstHalf = new("FIRST-HALF", 1);
    public static DayType LastHalf = new("LAST-HALF", 2);
    public static DayType Empty = new("EMPTY", 3);

    public DayType(string name, int value) : base(name, value)
    {}
}