namespace Itera.Fredrikstad.Presence.Core;

public record DayAtWork(
    string UserId,
    DateTime Date,
    DayType Type,
    string? Comment = null);
