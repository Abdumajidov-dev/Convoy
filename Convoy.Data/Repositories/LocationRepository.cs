using Convoy.Data.Context;
using Convoy.Data.Interfaces;
using Convoy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Convoy.Data.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly AppDbContext _context;

    public LocationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Location> CreateAsync(Location location)
    {
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();
        return location;
    }

    public async Task<IEnumerable<Location>> CreateBatchAsync(IEnumerable<Location> locations)
    {
        _context.Locations.AddRange(locations);
        await _context.SaveChangesAsync();
        return locations;
    }

    public async Task<IEnumerable<Location>> GetUserLocationsAsync(int userId, int limit = 100)
    {
        return await _context.Locations
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Location>> GetLatestLocationsAsync()
    {
        // GroupBy bilan Include ishlamaydi, shuning uchun ikki bosqichda qilamiz
        var latestLocationIds = await _context.Locations
            .GroupBy(l => l.UserId)
            .Select(g => g.OrderByDescending(l => l.Timestamp).Select(l => l.Id).FirstOrDefault())
            .ToListAsync();

        return await _context.Locations
            .Include(l => l.User)
            .Where(l => latestLocationIds.Contains(l.Id))
            .ToListAsync();
    }

    public async Task<IEnumerable<Location>> GetUserLocationsByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
    {
        return await _context.Locations
            .Include(l => l.User)
            .Where(l => l.UserId == userId
                && l.Timestamp >= startDate
                && l.Timestamp <= endDate)
            .OrderBy(l => l.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<Location>> GetAllUsersLocationsByDateAsync(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1).AddTicks(-1);

        return await _context.Locations
            .Include(l => l.User)
            .Where(l => l.Timestamp >= startOfDay && l.Timestamp <= endOfDay)
            .OrderBy(l => l.UserId)
            .ThenBy(l => l.Timestamp)
            .ToListAsync();
    }

    public async Task<Location?> GetLastLocationForUserAsync(int userId)
    {
        return await _context.Locations
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.Timestamp)
            .FirstOrDefaultAsync();
    }

    public async Task<Location?> GetLatestByUserIdAsync(int userId)
    {
        return await _context.Locations
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.Timestamp)
            .FirstOrDefaultAsync();
    }

    public async Task DeleteLocationsByDateAsync(DateTime beforeDate)
    {
        var locationsToDelete = await _context.Locations
            .Where(l => l.Timestamp < beforeDate)
            .ToListAsync();

        _context.Locations.RemoveRange(locationsToDelete);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserLocationsByDateAsync(int userId, DateTime beforeDate)
    {
        var locationsToDelete = await _context.Locations
            .Where(l => l.UserId == userId && l.Timestamp < beforeDate)
            .ToListAsync();

        _context.Locations.RemoveRange(locationsToDelete);
        await _context.SaveChangesAsync();
    }
}
