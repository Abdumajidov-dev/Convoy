using Convoy.Domain.Entities;

namespace Convoy.Data.Interfaces;

public interface IHourlySummaryRepository
{
    Task<List<HourlySummary>> CreateBatchAsync(List<HourlySummary> summaries);
    Task<List<HourlySummary>> GetByUserDateAndHoursAsync(int userId, DateTime date, List<int>? hours = null);
}
