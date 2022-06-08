namespace Itera.Fredrikstad.Presence.Web.Api.Models;

public record TeamEvent(string Name, DateTimeOffset Start, DateTimeOffset End, List<string> Attendees);
