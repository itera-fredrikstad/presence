namespace Itera.Fredrikstad.Presence.Core;

public interface IDayAtWorkRepository
{
    Task<List<DayAtWork>> GetAttendees(DateTime date);
    Task<List<DayAtWork>> GetDayAtWorkList(string userId);
    Task Update(DayAtWork dayAtWork);
}
