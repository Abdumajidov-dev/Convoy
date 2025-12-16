using Convoy.Data.Context;
using Convoy.Data.Interfaces;
using Convoy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Convoy.Data.Repositories;

public class HourlySummaryRepository : IHourlySummaryRepository
{
    private readonly AppDbContext _context;

    public HourlySummaryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<HourlySummary>> CreateBatchAsync(List<HourlySummary> summaries)
    {
        _context.HourlySummaries.AddRange(summaries);
        await _context.SaveChangesAsync();
        return summaries;
    }

    public async Task<List<HourlySummary>> GetByUserDateAndHoursAsync(int userId, DateTime date, List<int>? hours = null)
    {
        var dateOnly = date.Date;

        // Avval daily summary topamiz
        var dailySummary = await _context.DailySummaries
            .FirstOrDefaultAsync(ds => ds.UserId == userId && ds.Date == dateOnly);

        if (dailySummary == null)
        {
            return new List<HourlySummary>();
        }

        var query = _context.HourlySummaries
            .Include(hs => hs.User)
            .Where(hs => hs.DailySummaryId == dailySummary.Id);

        if (hours != null && hours.Any())
        {
            query = query.Where(hs => hours.Contains(hs.Hour));
        }

        return await query
            .OrderBy(hs => hs.Hour)
            .ToListAsync();
    }
}
