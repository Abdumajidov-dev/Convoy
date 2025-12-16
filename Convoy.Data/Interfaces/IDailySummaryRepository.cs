using Convoy.Domain.Entities;

namespace Convoy.Data.Interfaces;

public interface IDailySummaryRepository
{
    Task<DailySummary> CreateAsync(DailySummary summary);
    Task<DailySummary?> GetByIdAsync(int id, bool includeHourlySummaries = false);
    Task<DailySummary?> GetByUserAndDateAsync(int userId, DateTime date, bool includeHourlySummaries = false);
    Task<List<DailySummary>> GetByUserIdAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null, bool includeHourlySummaries = false);
    Task<bool> ExistsAsync(int userId, DateTime date);
}
