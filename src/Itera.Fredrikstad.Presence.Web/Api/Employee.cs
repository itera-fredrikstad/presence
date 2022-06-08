using Itera.Fredrikstad.Presence.Core;

namespace Itera.Fredrikstad.Presence.Web.Api;

public record Employee(string UserId, string name, List<DayAtWork> Days);
