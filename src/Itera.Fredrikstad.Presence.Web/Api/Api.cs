using System.Net;
using Ical.Net;
using Itera.Fredrikstad.Presence.Core;
using Itera.Fredrikstad.Presence.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using MinimalApis.Extensions.Results;

namespace Itera.Fredrikstad.Presence.Api;

public static class Api
{
    public static void MapApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("api/daySummary", GetDaySummary);
        app.MapGet("api/dayAtWork/{userId}", Get);
        app.MapPut("api/dayAtWork", Update);
        app.MapGet("api/teamEvents", GetEvents);
    }

    private static async Task<Ok<List<DayAtWork>>> Get(string userId, [FromServices] IDayAtWorkRepository repo)
    {
        var dayAtWorkList = await repo.GetDayAtWorkList(userId);
        return Results.Extensions.Ok(dayAtWorkList);
    }

    private static async Task<Ok<DaySummary>> GetDaySummary([FromQuery] DateTime date, [FromServices] IDayAtWorkRepository repo)
    {
        var attendees = await repo.GetAttendees(date);
        return Results.Extensions.Ok(
            new DaySummary(
                date,
                attendees
                    .Select(a => new DayAttendee(a.UserId, a.Type, a.Comment))
                    .ToList()));
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
        var events = calendar.Events.Select(e => new TeamEvent(e.Summary, e.Start.AsDateTimeOffset,
            e.End.AsDateTimeOffset, e.Attendees.Where(a => a.ParticipationStatus == "ACCEPTED").Select(a => a.CommonName ?? a.Value.ToString()).ToList())).ToList();

        return Results.Extensions.Ok(events.OrderBy(e => e.Start).ToList());
    }
}
