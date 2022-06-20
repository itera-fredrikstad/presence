using System.Net;
using Ical.Net;
using Itera.Fredrikstad.Presence.Core;
using Itera.Fredrikstad.Presence.Web.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MinimalApis.Extensions.Results;

namespace Itera.Fredrikstad.Presence.Web.Api.Endpoints;

public static class Api
{
    public static void MapApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("api/user", GetUser);
        app.MapGet("api/daySummary", GetDaySummary);
        app.MapGet("api/daySummaryRange", GetDaySummaryRange);
        app.MapGet("api/dayAtWork", GetDayAtWorks);
        app.MapPut("api/dayAtWork", Update);
        app.MapGet("api/teamEvents", GetEvents);
    }

    private static async Task<Results<Ok<User>, Unauthorized>> GetUser(HttpContext context, [FromServices] IDayAtWorkRepository repo)
    {
        if (context.User.Identity is { Name: { } } identity)
        {
            return Results.Extensions.Ok(new User(identity.Name, context.User.FindFirst(claim => claim.Type == "name")?.Value ?? "", context.User.FindFirst(claim => claim.Type == GraphClaimTypes.Photo)?.Value ?? ""));
        }

        return Results.Extensions.Unauthorized();
    }

    private static async Task<Results<Ok<List<DayAtWork>>, Unauthorized>> GetDayAtWorks(HttpContext context, [FromServices] IDayAtWorkRepository repo)
    {
        if (context.User.Identity is { Name: { } } identity)
        {
            return Results.Extensions.Ok(await repo.GetDayAtWorkList(identity.Name));
        }

        return Results.Extensions.Unauthorized();
    }

    [AllowAnonymous]
    private static async Task<Ok<DaySummary>> GetDaySummary([FromQuery] DateTime date, [FromServices] IDayAtWorkRepository repo, [FromServices] IMemoryCache cache)
    {
        var result = await cache.GetOrCreateAsync("summary-" + date.Date, async entry =>
        {
            entry.SetSlidingExpiration(TimeSpan.FromMinutes(60));
            return new DaySummary(
                date, (await repo.GetAttendees(date.Date))
                .Select(a => new DayAttendee(a.UserId, a.Type, a.Comment))
                .ToList());
        });
        
        return Results.Extensions.Ok(result);
    }
    
    [AllowAnonymous]
    private static async Task<Ok<List<DaySummary>>> GetDaySummaryRange([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromServices] IDayAtWorkRepository repo, [FromServices] IMemoryCache cache)
    {
        var datesToRetrieve = Enumerable
            .Range(0, 1 + toDate.Date.Subtract(fromDate.Date).Days)
            .Select(offset => fromDate.Date.AddDays(offset));

        var result = await datesToRetrieve
            .SelectAsync(date => cache
                .GetOrCreateAsync(
                    "summary-" + date,
                    async entry =>
                    {
                        entry.SetSlidingExpiration(TimeSpan.FromMinutes(60));
                        return new DaySummary(
                            date, (await repo.GetAttendees(date))
                            .Select(a => new DayAttendee(a.UserId, a.Type, a.Comment))
                            .ToList());
                    }))
            .ToListAsync();

        return Results.Extensions.Ok(result);
    }

    private static async Task<Ok<DayAtWork>> Update([FromBody] DayAtWork dayAtWork, [FromServices] IDayAtWorkRepository repo, [FromServices] IMemoryCache cache)
    {
        await repo.Update(dayAtWork);
        
        cache.Remove("summary-" + dayAtWork.Date.Date);
        
        return Results.Extensions.Ok(dayAtWork);
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
