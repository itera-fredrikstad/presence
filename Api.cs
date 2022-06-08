using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Claims;
using Ardalis.SmartEnum;
using Ical.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApis.Extensions.Results;

namespace Itera.Fredrikstad.Presence;

public record Employee(string UserId, string name, List<DayAtWork> Days);

public record DayAttendee(string UserId, DayType Type, string? Comment = null);
public record DaySummary(DateTime Date, List<DayAttendee> Attendees);

public static class Api
{
    public static void MapApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("api/daySummary", GetDaySummary);
        app.MapGet("api/dayAtWork", Get);
        app.MapPut("api/dayAtWork", Update);
        app.MapGet("api/teamEvents", GetEvents);
    }

    private static async Task<Ok<Employee>> Get(HttpContext context, [FromServices] Db db)
    {
        var user = context.User.Identity?.Name ?? "";
        var name = context.User.FindFirst(claim => claim.Type == "name")?.Value ?? "";
        var days = await db.DayAtWorks.AsNoTracking().Where(d => d.UserId.Equals(user) && d.Date >= DateTime.Today).ToListAsync();
        return Results.Extensions.Ok(new Employee(user, name, days));
    }

    [AllowAnonymous]
    private static async Task<Ok<DaySummary>> GetDaySummary([FromQuery] DateTime date, [FromServices] Db db)
    {
        var attendees = await db.DayAtWorks.AsNoTracking().Where(d => d.Date >= date && d.Date < date.AddDays(1)).ToListAsync();
        return Results.Extensions.Ok(new DaySummary(date, attendees.Select(a => new DayAttendee(a.UserId, a.Type, a.Comment)).ToList()));
    }

    private static async Task<Ok> Update([FromBody] DayAtWork dayAtWork, [FromServices] Db db)
    {
        await db.AddOrUpdate(dayAtWork, d => new object[] { d.UserId, d.Date });
        await db.SaveChangesAsync();

        return Results.Extensions.Ok();
    }


    private static async Task<Ok<List<TeamEvent>>> GetEvents([FromServices] IConfiguration config)
    {
        var calendar = Calendar.Load(new WebClient().DownloadString(config.GetValue<string>("CAL_URL")));

        var events = calendar.Events.SelectMany(e => e
            .GetOccurrences(DateTime.Today, DateTime.Today.AddYears(1))
            .Select(o => new TeamEvent(e.Summary, o.Period.StartTime.AsDateTimeOffset, o.Period.EndTime.AsDateTimeOffset, new())))
            .OrderBy(e => e.Start);

        return Results.Extensions.Ok(events.ToList());
    }
}

public record TeamEvent(string Name, DateTimeOffset Start, DateTimeOffset End, List<string> Attendees);

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
    { }
}