namespace Itera.Fredrikstad.Presence.Web.Api.Models;

public record DaySummary(DateTime Date, List<DayAttendee> Attendees);
