using Itera.Fredrikstad.Presence.Core;
using Microsoft.EntityFrameworkCore;

namespace Itera.Fredrikstad.Presence.Infrastructure;

public class DayAtWorkSqlRepository : IDayAtWorkRepository
{
    private readonly Db _db;

    public DayAtWorkSqlRepository(Db db)
    {
        _db = db;
    }

    public async Task<List<DayAtWork>> GetAttendees(DateTime date)
    {
        var attendees = await _db.DayAtWorks.AsNoTracking()
            .Where(d => d.Date >= date && d.Date < date.AddDays(1))
            .ToListAsync();

        return attendees;
    }

    public async Task<List<DayAtWork>> GetDayAtWorkList(string userId)
    {
        var dayAtWork = await _db.DayAtWorks.AsNoTracking()
            .Where(d => d.UserId.Equals(userId) && d.Date >= DateTime.Today)
            .ToListAsync();

        return dayAtWork;
    }

    public async Task Update(DayAtWork dayAtWork)
    {
        await _db.AddOrUpdate(dayAtWork, d => new object[] { d.UserId, d.Date });
        await _db.SaveChangesAsync();
    }
}