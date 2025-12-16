using Convoy.Data.Interfaces;
using Convoy.Domain.Entities;
using Convoy.Service.Interfaces;

namespace Convoy.Service.Services;

public class LocationService : ILocationService
{
    private readonly ILocationRepository _locationRepository;
    private readonly IUserRepository _userRepository;

    public LocationService(ILocationRepository locationRepository, IUserRepository userRepository)
    {
        _locationRepository = locationRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<Location>> GetUserLocationsAsync(int userId, int limit = 100)
    {
        return await _locationRepository.GetUserLocationsAsync(userId, limit);
    }

    public async Task<IEnumerable<Location>> GetLatestLocationsAsync()
    {
        return await _locationRepository.GetLatestLocationsAsync();
    }

    // SignalR uchun bitta location qo'shish
    public async Task<Location?> AddLocationAsync(Location location)
    {
        try
        {
            // User mavjudligini tekshirish
            var userExists = await _userRepository.ExistsAsync(location.UserId);
            if (!userExists)
            {
                return null;
            }

            // Location saqlash
            return await _locationRepository.CreateAsync(location);
        }
        catch
        {
            return null;
        }
    }

    // Smart location saving - 30m ichida turgan bo'lsa saqlamaydi
    public async Task<Location?> AddLocationWithDeduplicationAsync(Location location)
    {
        try
        {
            // User mavjudligini tekshirish
            var userExists = await _userRepository.ExistsAsync(location.UserId);
            if (!userExists)
            {
                return null;
            }

            // Oxirgi locationni olish
            var lastLocation = await _locationRepository.GetLastLocationForUserAsync(location.UserId);

            if (lastLocation != null)
            {
                // Masofa hisoblab ko'rish (Haversine formula)
                var distance = CalculateDistance(
                    lastLocation.Latitude, lastLocation.Longitude,
                    location.Latitude, location.Longitude);

                // Agar 30 metrdan kam bo'lsa va speed = 0 bo'lsa, saqlamaslik
                if (distance < 30 && location.Speed == 0)
                {
                    // Duplicate deb qaytaramiz (saqlangan location)
                    return lastLocation;
                }
            }

            // Yangi location saqlaymiz
            return await _locationRepository.CreateAsync(location);
        }
        catch
        {
            return null;
        }
    }

    public async Task<IEnumerable<Location>> GetUserLocationsByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
    {
        return await _locationRepository.GetUserLocationsByDateRangeAsync(userId, startDate, endDate);
    }

    public async Task<IEnumerable<Location>> GetAllUsersLocationsByDateAsync(DateTime date)
    {
        return await _locationRepository.GetAllUsersLocationsByDateAsync(date);
    }

    // Haversine formula - ikki GPS nuqta orasidagi masofani hisoblab beradi (metrda)
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000; // Yer radiusi metrda
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c; // Masofa metrda
    }

    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
