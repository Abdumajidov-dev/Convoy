using Convoy.Data.Interfaces;
using Convoy.Domain.DTOs;
using Convoy.Domain.Entities;
using Convoy.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace Convoy.Service.Services;

public class DailySummaryService : IDailySummaryService
{
    private readonly IDailySummaryRepository _dailySummaryRepository;
    private readonly IHourlySummaryRepository _hourlySummaryRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DailySummaryService> _logger;

    public DailySummaryService(
        IDailySummaryRepository dailySummaryRepository,
        IHourlySummaryRepository hourlySummaryRepository,
        ILocationRepository locationRepository,
        IUserRepository userRepository,
        ILogger<DailySummaryService> logger)
    {
        _dailySummaryRepository = dailySummaryRepository;
        _hourlySummaryRepository = hourlySummaryRepository;
        _locationRepository = locationRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<DailySummaryDto> CreateDailySummaryAsync(int userId, DateTime date)
    {
        var dateOnly = date.Date;

        // Agar allaqachon summary mavjud bo'lsa, uni qaytaramiz
        var existing = await _dailySummaryRepository.GetByUserAndDateAsync(userId, dateOnly, true);
        if (existing != null)
        {
            _logger.LogInformation($"Daily summary already exists for user {userId} on {dateOnly:yyyy-MM-dd}");
            return MapToDto(existing);
        }

        _logger.LogInformation($"Creating daily summary for user {userId} on {dateOnly:yyyy-MM-dd}");

        // O'sha kunning location'larini olish
        var startOfDay = dateOnly;
        var endOfDay = dateOnly.AddDays(1);
        var locations = (await _locationRepository.GetUserLocationsByDateRangeAsync(userId, startOfDay, endOfDay)).ToList();

        if (!locations.Any())
        {
            _logger.LogWarning($"No locations found for user {userId} on {dateOnly:yyyy-MM-dd}");
            throw new InvalidOperationException($"No locations found for user {userId} on {dateOnly:yyyy-MM-dd}");
        }

        // Daily summary yaratish
        var dailySummary = new DailySummary
        {
            UserId = userId,
            Date = dateOnly,
            TotalLocations = locations.Count,
            FirstLocationTime = locations.Min(l => l.Timestamp),
            LastLocationTime = locations.Max(l => l.Timestamp),
            CreatedAt = DateTime.UtcNow
        };

        // Masofa va tezlik hisoblash
        CalculateDistanceAndSpeed(locations, dailySummary);

        // Geografik chegara hisoblash
        CalculateGeographicBounds(locations, dailySummary);

        // Daily summary saqlash
        var savedSummary = await _dailySummaryRepository.CreateAsync(dailySummary);

        // Soatlik summary yaratish
        var hourlySummaries = CreateHourlySummaries(locations, savedSummary.Id, userId);
        if (hourlySummaries.Any())
        {
            await _hourlySummaryRepository.CreateBatchAsync(hourlySummaries);
        }

        // Summary'ni qayta olish (hourly summaries bilan)
        var result = await _dailySummaryRepository.GetByIdAsync(savedSummary.Id, true);

        _logger.LogInformation($"Daily summary created successfully for user {userId} on {dateOnly:yyyy-MM-dd}");

        return MapToDto(result!);
    }

    public async Task ProcessYesterdayDataAsync()
    {
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);

        _logger.LogInformation($"Processing yesterday's data ({yesterday:yyyy-MM-dd}) for all users");

        // Barcha aktiv user'larni olish
        var users = await _userRepository.GetAllAsync();
        var activeUsers = users.Where(u => u.IsActive).ToList();

        foreach (var user in activeUsers)
        {
            try
            {
                // Agar user'ning o'sha kuni location'lari bo'lsa, summary yaratamiz
                var startOfDay = yesterday;
                var endOfDay = yesterday.AddDays(1);
                var locations = await _locationRepository.GetUserLocationsByDateRangeAsync(user.Id, startOfDay, endOfDay);

                if (locations.Any())
                {
                    await CreateDailySummaryAsync(user.Id, yesterday);
                    _logger.LogInformation($"Summary created for user {user.Id} - {user.Name}");
                }
                else
                {
                    _logger.LogInformation($"No locations for user {user.Id} - {user.Name} on {yesterday:yyyy-MM-dd}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating summary for user {user.Id} - {user.Name}");
            }
        }

        _logger.LogInformation("Yesterday's data processing completed");
    }

    public async Task<DailySummaryDto?> GetDailySummaryAsync(int userId, DateTime date, bool includeHourlySummaries = true)
    {
        var summary = await _dailySummaryRepository.GetByUserAndDateAsync(userId, date.Date, includeHourlySummaries);
        return summary != null ? MapToDto(summary) : null;
    }

    public async Task<List<DailySummaryDto>> GetUserSummariesAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var summaries = await _dailySummaryRepository.GetByUserIdAsync(userId, fromDate, toDate, true);
        return summaries.Select(MapToDto).ToList();
    }

    public async Task<List<HourlySummaryDto>> GetHourlySummariesAsync(int userId, DateTime date, List<int>? hours = null)
    {
        var hourlySummaries = await _hourlySummaryRepository.GetByUserDateAndHoursAsync(userId, date.Date, hours);
        return hourlySummaries.Select(MapHourlySummaryToDto).ToList();
    }

    public async Task CleanupOldLocationsAsync(int daysToKeep = 7)
    {
        var cutoffDate = DateTime.UtcNow.Date.AddDays(-daysToKeep);

        _logger.LogInformation($"Cleaning up locations older than {cutoffDate:yyyy-MM-dd}");

        await _locationRepository.DeleteLocationsByDateAsync(cutoffDate);

        _logger.LogInformation("Old locations cleanup completed");
    }

    private void CalculateDistanceAndSpeed(List<Location> locations, DailySummary summary)
    {
        double totalDistance = 0;
        double totalSpeed = 0;
        int speedCount = 0;
        double maxSpeed = 0;

        for (int i = 1; i < locations.Count; i++)
        {
            var prev = locations[i - 1];
            var current = locations[i];

            // Masofa hisoblash (Haversine formula)
            var distance = CalculateDistance(
                prev.Latitude, prev.Longitude,
                current.Latitude, current.Longitude
            );
            totalDistance += distance;

            // Tezlik hisoblash
            if (current.Speed.HasValue)
            {
                totalSpeed += current.Speed.Value;
                speedCount++;

                if (current.Speed.Value > maxSpeed)
                {
                    maxSpeed = current.Speed.Value;
                }
            }
        }

        summary.TotalDistanceKm = totalDistance;
        summary.AverageSpeed = speedCount > 0 ? totalSpeed / speedCount : null;
        summary.MaxSpeed = maxSpeed > 0 ? maxSpeed : null;
    }

    private void CalculateGeographicBounds(List<Location> locations, DailySummary summary)
    {
        summary.MinLatitude = locations.Min(l => l.Latitude);
        summary.MaxLatitude = locations.Max(l => l.Latitude);
        summary.MinLongitude = locations.Min(l => l.Longitude);
        summary.MaxLongitude = locations.Max(l => l.Longitude);
    }

    private List<HourlySummary> CreateHourlySummaries(List<Location> locations, int dailySummaryId, int userId)
    {
        var hourlySummaries = new List<HourlySummary>();

        // Location'larni soatga qarab guruhlash
        var locationsByHour = locations
            .GroupBy(l => l.Timestamp.Hour)
            .OrderBy(g => g.Key);

        foreach (var hourGroup in locationsByHour)
        {
            var hourLocations = hourGroup.ToList();

            var hourlySummary = new HourlySummary
            {
                DailySummaryId = dailySummaryId,
                UserId = userId,
                Hour = hourGroup.Key,
                LocationCount = hourLocations.Count,
                FirstLocationTime = hourLocations.Min(l => l.Timestamp),
                LastLocationTime = hourLocations.Max(l => l.Timestamp)
            };

            // Masofa va tezlik hisoblash
            double totalDistance = 0;
            double totalSpeed = 0;
            int speedCount = 0;

            for (int i = 1; i < hourLocations.Count; i++)
            {
                var prev = hourLocations[i - 1];
                var current = hourLocations[i];

                var distance = CalculateDistance(
                    prev.Latitude, prev.Longitude,
                    current.Latitude, current.Longitude
                );
                totalDistance += distance;

                if (current.Speed.HasValue)
                {
                    totalSpeed += current.Speed.Value;
                    speedCount++;
                }
            }

            hourlySummary.DistanceKm = totalDistance;
            hourlySummary.AverageSpeed = speedCount > 0 ? totalSpeed / speedCount : null;

            // Geografik chegara
            hourlySummary.MinLatitude = hourLocations.Min(l => l.Latitude);
            hourlySummary.MaxLatitude = hourLocations.Max(l => l.Latitude);
            hourlySummary.MinLongitude = hourLocations.Min(l => l.Longitude);
            hourlySummary.MaxLongitude = hourLocations.Max(l => l.Longitude);

            hourlySummaries.Add(hourlySummary);
        }

        return hourlySummaries;
    }

    private DailySummaryDto MapToDto(DailySummary summary)
    {
        return new DailySummaryDto
        {
            Id = summary.Id,
            UserId = summary.UserId,
            UserName = summary.User?.Name ?? string.Empty,
            Date = summary.Date,
            TotalLocations = summary.TotalLocations,
            FirstLocationTime = summary.FirstLocationTime,
            LastLocationTime = summary.LastLocationTime,
            TotalDistanceKm = summary.TotalDistanceKm,
            AverageSpeed = summary.AverageSpeed,
            MaxSpeed = summary.MaxSpeed,
            MinLatitude = summary.MinLatitude,
            MaxLatitude = summary.MaxLatitude,
            MinLongitude = summary.MinLongitude,
            MaxLongitude = summary.MaxLongitude,
            HourlySummaries = summary.HourlySummaries?.Select(MapHourlySummaryToDto).ToList() ?? new List<HourlySummaryDto>()
        };
    }

    private HourlySummaryDto MapHourlySummaryToDto(HourlySummary summary)
    {
        return new HourlySummaryDto
        {
            Id = summary.Id,
            Hour = summary.Hour,
            LocationCount = summary.LocationCount,
            DistanceKm = summary.DistanceKm,
            AverageSpeed = summary.AverageSpeed,
            FirstLocationTime = summary.FirstLocationTime,
            LastLocationTime = summary.LastLocationTime,
            MinLatitude = summary.MinLatitude,
            MaxLatitude = summary.MaxLatitude,
            MinLongitude = summary.MinLongitude,
            MaxLongitude = summary.MaxLongitude
        };
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371.0;

        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}
