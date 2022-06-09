using System.Net;
using Ical.Net;
using Itera.Fredrikstad.Presence.Core;
using Itera.Fredrikstad.Presence.Web.Api.Models;
using Microsoft.AspNetCore.Mvc;
using MinimalApis.Extensions.Results;

namespace Itera.Fredrikstad.Presence.Web.Api.Endpoints;

public static class Api
{
    public static void MapApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("api/user", GetUser);
        app.MapGet("api/daySummary", GetDaySummary);
        app.MapGet("api/dayAtWork", GetDayAtWorks);
        app.MapPut("api/dayAtWork", Update);
        app.MapGet("api/teamEvents", GetEvents);
    }

    private static async Task<Results<Ok<User>, Unauthorized>> GetUser(HttpContext context, [FromServices] IDayAtWorkRepository repo)
    {
        if(context.User.Identity is { Name: {} } identity) 
        {
            return Results.Extensions.Ok(new User(identity.Name, context.User.FindFirst(claim => claim.Type == "name")?.Value ?? ""));
        }

        return Results.Extensions.Unauthorized();
    }

    private static async Task<Results<Ok<List<DayAtWork>>, Unauthorized>> GetDayAtWorks(HttpContext context, [FromServices] IDayAtWorkRepository repo) 
    {
        if(context.User.Identity is { Name: {} } identity)
        {
            return Results.Extensions.Ok(await repo.GetDayAtWorkList(identity.Name));
        }
        
        return Results.Extensions.Unauthorized();
    } 

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
