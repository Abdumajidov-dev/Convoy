using Convoy.Domain.DTOs;

namespace Convoy.Service.Interfaces;

public interface IDailySummaryService
{
    /// <summary>
    /// Ma'lum kun uchun daily summary yaratish (background job ishlatadi)
    /// </summary>
    Task<DailySummaryDto> CreateDailySummaryAsync(int userId, DateTime date);

    /// <summary>
    /// Barcha active user'lar uchun kecha sanasining summary'sini yaratish
    /// </summary>
    Task ProcessYesterdayDataAsync();

    /// <summary>
    /// User'ning ma'lum kundagi summary'sini olish
    /// </summary>
    Task<DailySummaryDto?> GetDailySummaryAsync(int userId, DateTime date, bool includeHourlySummaries = true);

    /// <summary>
    /// User'ning sana oralig'idagi summary'larini olish
    /// </summary>
    Task<List<DailySummaryDto>> GetUserSummariesAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Haritada ko'rsatish uchun ma'lum soatlarning location'larini olish
    /// </summary>
    Task<List<HourlySummaryDto>> GetHourlySummariesAsync(int userId, DateTime date, List<int>? hours = null);

    /// <summary>
    /// Eski location'larni o'chirish (summary yaratilganidan keyin)
    /// </summary>
    Task CleanupOldLocationsAsync(int daysToKeep = 7);
}
