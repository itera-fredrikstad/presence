namespace Itera.Fredrikstad.Presence.Web.Api;

public record TeamEvent(string Name, DateTimeOffset Start, DateTimeOffset End, List<string> Attendees);
