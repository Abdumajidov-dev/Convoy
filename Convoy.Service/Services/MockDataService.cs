using Convoy.Data.Interfaces;
using Convoy.Domain.DTOs;
using Convoy.Domain.Entities;
using Convoy.Service.Interfaces;

namespace Convoy.Service.Services;

public class MockDataService : IMockDataService
{
    private readonly ILocationRepository _locationRepository;
    private readonly IUserRepository _userRepository;
    private readonly Random _random = new Random();

    // Andijon shahri markazida lokatsiyalar
    private readonly (double Lat, double Lng)[] _locations = new[]
    {
        (40.7821, 72.3442), // Office
        (40.7900, 72.3550), // Customer 1
        (40.7950, 72.3600), // Customer 2
        (40.7750, 72.3350), // Customer 3
        (40.7680, 72.3450), // Customer 4
        (40.7870, 72.3620), // Restaurant
        (40.7780, 72.3300), // Customer 5
        (40.7820, 72.3500)  // Customer 6
    };

    public MockDataService(ILocationRepository locationRepository, IUserRepository userRepository)
    {
        _locationRepository = locationRepository;
        _userRepository = userRepository;
    }

    public async Task<MockLocationResponse> GenerateMockLocationsAsync(MockLocationRequest request)
    {
        // User mavjudligini tekshirish
        var userExists = await _userRepository.ExistsAsync(request.UserId);
        if (!userExists)
        {
            return new MockLocationResponse
            {
                Success = false,
                Message = $"User ID {request.UserId} topilmadi"
            };
        }

        var allLocations = new List<Location>();
        var currentDate = request.StartDate.Date;
        int dayCount = 0;
        int visitCount = 0;

        // Har bir kun uchun data generatsiya qilish
        while (currentDate <= request.EndDate.Date)
        {
            var dailyLocations = GenerateDailyRoute(request.UserId, currentDate);
            allLocations.AddRange(dailyLocations);

            dayCount++;
            visitCount += 7; // Har kuni 7 ta tashrif

            currentDate = currentDate.AddDays(1);
        }

        // Barchasini database ga saqlash
        await _locationRepository.CreateBatchAsync(allLocations);

        return new MockLocationResponse
        {
            Success = true,
            Message = $"User {request.UserId} uchun {dayCount} kunlik mock data yaratildi",
            PointsGenerated = allLocations.Count,
            DaysGenerated = dayCount,
            EstimatedVisits = visitCount
        };
    }

    private List<Location> GenerateDailyRoute(int userId, DateTime date)
    {
        var locations = new List<Location>();
        var currentTime = date.AddHours(9); // 09:00 dan boshlanadi

        // 09:00-09:30: Office da turish
        locations.AddRange(GenerateStationarySegment(
            userId, _locations[0], currentTime, 30));
        currentTime = currentTime.AddMinutes(30);

        // 09:30-10:00: Office dan Customer 1 ga borish
        locations.AddRange(GenerateDrivingSegment(
            userId, _locations[0], _locations[1], currentTime, 30));
        currentTime = currentTime.AddMinutes(30);

        // 10:00-10:40: Customer 1 da turish (40 min)
        locations.AddRange(GenerateStationarySegment(
            userId, _locations[1], currentTime, 40));
        currentTime = currentTime.AddMinutes(40);

        // 10:40-11:00: Customer 1 dan Customer 2 ga borish
        locations.AddRange(GenerateDrivingSegment(
            userId, _locations[1], _locations[2], currentTime, 20));
        currentTime = currentTime.AddMinutes(20);

        // 11:00-11:30: Customer 2 da turish (30 min)
        locations.AddRange(GenerateStationarySegment(
            userId, _locations[2], currentTime, 30));
        currentTime = currentTime.AddMinutes(30);

        // 11:30-11:45: Customer 2 dan Restaurant ga borish
        locations.AddRange(GenerateDrivingSegment(
            userId, _locations[2], _locations[5], currentTime, 15));
        currentTime = currentTime.AddMinutes(15);

        // 11:45-12:45: Tushlik (60 min)
        locations.AddRange(GenerateStationarySegment(
            userId, _locations[5], currentTime, 60));
        currentTime = currentTime.AddMinutes(60);

        // 12:45-13:10: Restaurant dan Customer 3 ga borish
        locations.AddRange(GenerateDrivingSegment(
            userId, _locations[5], _locations[3], currentTime, 25));
        currentTime = currentTime.AddMinutes(25);

        // 13:10-14:00: Customer 3 da turish (50 min)
        locations.AddRange(GenerateStationarySegment(
            userId, _locations[3], currentTime, 50));
        currentTime = currentTime.AddMinutes(50);

        // 14:00-14:20: Customer 3 dan Customer 4 ga borish
        locations.AddRange(GenerateDrivingSegment(
            userId, _locations[3], _locations[4], currentTime, 20));
        currentTime = currentTime.AddMinutes(20);

        // 14:20-15:05: Customer 4 da turish (45 min)
        locations.AddRange(GenerateStationarySegment(
            userId, _locations[4], currentTime, 45));
        currentTime = currentTime.AddMinutes(45);

        // 15:05-15:25: Customer 4 dan Customer 5 ga borish
        locations.AddRange(GenerateDrivingSegment(
            userId, _locations[4], _locations[6], currentTime, 20));
        currentTime = currentTime.AddMinutes(20);

        // 15:25-16:15: Customer 5 da turish (50 min)
        locations.AddRange(GenerateStationarySegment(
            userId, _locations[6], currentTime, 50));
        currentTime = currentTime.AddMinutes(50);

        // 16:15-17:00: Customer 5 dan Office/Uyga qaytish
        locations.AddRange(GenerateDrivingSegment(
            userId, _locations[6], _locations[0], currentTime, 45));

        return locations;
    }

    private List<Location> GenerateStationarySegment(
        int userId, (double Lat, double Lng) center, DateTime startTime, int durationMinutes)
    {
        var locations = new List<Location>();

        for (int i = 0; i < durationMinutes; i++)
        {
            locations.Add(new Location
            {
                UserId = userId,
                Latitude = center.Lat + (_random.NextDouble() * 0.0004 - 0.0002), // ±20m
                Longitude = center.Lng + (_random.NextDouble() * 0.0004 - 0.0002),
                Timestamp = startTime.AddMinutes(i),
                Speed = 0,
                Accuracy = _random.Next(5, 15)
            });
        }

        return locations;
    }

    private List<Location> GenerateDrivingSegment(
        int userId, (double Lat, double Lng) from, (double Lat, double Lng) to,
        DateTime startTime, int durationMinutes)
    {
        var locations = new List<Location>();
        int pointsPerMinute = 2; // Harakatda 2 point/minute
        int totalPoints = durationMinutes * pointsPerMinute;

        for (int i = 0; i < totalPoints; i++)
        {
            double progress = (double)i / totalPoints;

            locations.Add(new Location
            {
                UserId = userId,
                Latitude = Interpolate(from.Lat, to.Lat, progress) + (_random.NextDouble() * 0.0002 - 0.0001),
                Longitude = Interpolate(from.Lng, to.Lng, progress) + (_random.NextDouble() * 0.0002 - 0.0001),
                Timestamp = startTime.AddSeconds(i * 30), // Har 30 sekundda
                Speed = _random.Next(20, 60),
                Accuracy = _random.Next(5, 20)
            });
        }

        return locations;
    }

    private double Interpolate(double start, double end, double progress)
    {
        return start + (end - start) * progress;
    }

    public async Task<int> ClearUserLocationsAsync(int userId)
    {
        // Bu methodning to'liq implementatsiyasi kerak bo'lsa qo'shamiz
        // Hozircha placeholder
        return await Task.FromResult(0);
    }
}
