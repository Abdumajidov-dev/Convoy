using Convoy.Domain.Entities;

namespace Convoy.Data.Interfaces;

public interface ILocationRepository
{
    Task<Location> CreateAsync(Location location);
    Task<IEnumerable<Location>> CreateBatchAsync(IEnumerable<Location> locations);
    Task<IEnumerable<Location>> GetUserLocationsAsync(int userId, int limit = 100);
    Task<IEnumerable<Location>> GetLatestLocationsAsync();

    // Yangi methodlar
    Task<IEnumerable<Location>> GetUserLocationsByDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Location>> GetAllUsersLocationsByDateAsync(DateTime date);
    Task<Location?> GetLastLocationForUserAsync(int userId);
    Task<Location?> GetLatestByUserIdAsync(int userId);

    // Old location cleanup
    Task DeleteLocationsByDateAsync(DateTime beforeDate);
    Task DeleteUserLocationsByDateAsync(int userId, DateTime beforeDate);
}
