namespace Itera.Fredrikstad.Presence.Api;

public record DaySummary(DateTime Date, List<DayAttendee> Attendees);
