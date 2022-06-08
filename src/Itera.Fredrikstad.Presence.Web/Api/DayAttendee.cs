using Itera.Fredrikstad.Presence.Core;

namespace Itera.Fredrikstad.Presence.Web.Api;

public record DayAttendee(string UserId, DayType Type, string? Comment = null);
