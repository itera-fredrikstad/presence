namespace Itera.Fredrikstad.Presence.Api;

public record TeamEvent(string Name, DateTimeOffset Start, DateTimeOffset End, List<string> Attendees);
