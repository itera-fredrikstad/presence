using Itera.Fredrikstad.Presence.Core;

namespace Itera.Fredrikstad.Presence.Api;

public record DayAttendee(string UserId, DayType Type, string? Comment = null);
