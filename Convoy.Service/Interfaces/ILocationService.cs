using Convoy.Domain.Entities;

namespace Convoy.Service.Interfaces;

public interface ILocationService
{
    Task<Location?> AddLocationAsync(Location location); // SignalR va REST API uchun
    Task<Location?> AddLocationWithDeduplicationAsync(Location location); // Smart saving with 30m threshold
    Task<IEnumerable<Location>> GetUserLocationsAsync(int userId, int limit = 100);
    Task<IEnumerable<Location>> GetLatestLocationsAsync();

    // Yangi methodlar
    Task<IEnumerable<Location>> GetUserLocationsByDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Location>> GetAllUsersLocationsByDateAsync(DateTime date);
}
