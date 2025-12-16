using Convoy.Data.Context;
using Convoy.Data.Interfaces;
using Convoy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Convoy.Data.Repositories;

public class DailySummaryRepository : IDailySummaryRepository
{
    private readonly AppDbContext _context;

    public DailySummaryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DailySummary> CreateAsync(DailySummary summary)
    {
        _context.DailySummaries.Add(summary);
        await _context.SaveChangesAsync();
        return summary;
    }

    public async Task<DailySummary?> GetByIdAsync(int id, bool includeHourlySummaries = false)
    {
        var query = _context.DailySummaries
            .Include(ds => ds.User)
            .AsQueryable();

        if (includeHourlySummaries)
        {
            query = query.Include(ds => ds.HourlySummaries.OrderBy(hs => hs.Hour));
        }

        return await query.FirstOrDefaultAsync(ds => ds.Id == id);
    }

    public async Task<DailySummary?> GetByUserAndDateAsync(int userId, DateTime date, bool includeHourlySummaries = false)
    {
        var dateOnly = date.Date;

        var query = _context.DailySummaries
            .Include(ds => ds.User)
            .AsQueryable();

        if (includeHourlySummaries)
        {
            query = query.Include(ds => ds.HourlySummaries.OrderBy(hs => hs.Hour));
        }

        return await query.FirstOrDefaultAsync(ds => ds.UserId == userId && ds.Date == dateOnly);
    }

    public async Task<List<DailySummary>> GetByUserIdAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null, bool includeHourlySummaries = false)
    {
        var query = _context.DailySummaries
            .Include(ds => ds.User)
            .AsQueryable();

        if (includeHourlySummaries)
        {
            query = query.Include(ds => ds.HourlySummaries.OrderBy(hs => hs.Hour));
        }

        query = query.Where(ds => ds.UserId == userId);

        if (fromDate.HasValue)
        {
            var fromDateOnly = fromDate.Value.Date;
            query = query.Where(ds => ds.Date >= fromDateOnly);
        }

        if (toDate.HasValue)
        {
            var toDateOnly = toDate.Value.Date;
            query = query.Where(ds => ds.Date <= toDateOnly);
        }

        return await query
            .OrderByDescending(ds => ds.Date)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int userId, DateTime date)
    {
        var dateOnly = date.Date;
        return await _context.DailySummaries
            .AnyAsync(ds => ds.UserId == userId && ds.Date == dateOnly);
    }
}
