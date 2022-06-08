using Ardalis.SmartEnum;

namespace Itera.Fredrikstad.Presence.Core;

public class DayType : SmartEnum<DayType>
{
    public static DayType Full = new("FULL", 0);
    public static DayType FirstHalf = new("FIRST-HALF", 1);
    public static DayType LastHalf = new("LAST-HALF", 2);
    public static DayType Empty = new("EMPTY", 3);

    public DayType(string name, int value) : base(name, value)
    { }
}