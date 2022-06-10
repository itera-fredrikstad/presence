using System.Net;
using Ical.Net;
using Itera.Fredrikstad.Presence.Core;
using Itera.Fredrikstad.Presence.Web.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinimalApis.Extensions.Results;

namespace Itera.Fredrikstad.Presence.Web.Api.Endpoints;

public static class Api
{
    public static void MapApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("api/daySummary", GetDaySummary);
        app.MapGet("api/dayAtWork", Get);
        app.MapPut("api/dayAtWork", Update);
        app.MapGet("api/teamEvents", GetEvents);
        app.MapGet("api/user", GetUser);
    }


    private static IResult GetUser(HttpContext context)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            return Results.Extensions.Unauthorized();
        }
        var user = context.User.Identity?.Name ?? "";
        var name = context.User.FindFirst(claim => claim.Type == "name")?.Value ?? "";
        var photo = context.User.FindFirst(claim => claim.Type == GraphClaimTypes.Photo)?.Value ?? "";
        return Results.Extensions.Ok(new User(user, name, photo));
    }

    private static async Task<Ok<List<DayAtWork>>> Get(HttpContext context, [FromServices] IDayAtWorkRepository repo)
    {
        var user = context.User.Identity?.Name ?? "";
        var days = await repo.GetDayAtWorkList(user);
        return Results.Extensions.Ok(days);
    }

    [AllowAnonymous]
    private static async Task<Ok<DaySummary>> GetDaySummary([FromQuery] DateTime date, [FromServices] IDayAtWorkRepository repo)
    {
        var attendees = await repo.GetAttendees(date);
        return Results.Extensions.Ok(new DaySummary(date, attendees.Select(a => new DayAttendee(a.UserId, a.Type, a.Comment)).ToList()));
    }

    private static async Task<Ok> Update([FromBody] DayAtWork dayAtWork, [FromServices] IDayAtWorkRepository repo)
    {
        await repo.Update(dayAtWork);

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
