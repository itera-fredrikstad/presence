namespace Itera.Fredrikstad.Presence.Web.Api;

public record DaySummary(DateTime Date, List<DayAttendee> Attendees);
