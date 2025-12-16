using Convoy.Domain.DTOs;
using Convoy.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Convoy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly ILocationService _locationService;
    private readonly ILogger<LocationController> _logger;

    public LocationController(ILocationService locationService, ILogger<LocationController> logger)
    {
        _locationService = locationService;
        _logger = logger;
    }

    /// <summary>
    /// POST: api/location
    /// Accepts: { "userId": 1, "locations": [{lat, lng, timestamp, speed, accuracy}, ...] }
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> PostLocations([FromBody] LocationDto? locationDto)
    {
        // Log raw request body
        Request.EnableBuffering();
        using (var reader = new StreamReader(Request.Body, leaveOpen: true))
        {
            var body = await reader.ReadToEndAsync();
            Request.Body.Position = 0;
            _logger.LogInformation("Received request body: {Body}", body);
            _logger.LogInformation("Content-Type: {ContentType}", Request.ContentType);
        }

        if (locationDto == null)
        {
            _logger.LogWarning("LocationDto is null");
            return BadRequest(new { message = "Request body bo'sh bo'lmasligi kerak" });
        }

        if (locationDto.Locations == null || !locationDto.Locations.Any())
        {
            _logger.LogWarning("Locations array is null or empty");
            return BadRequest(new { message = "Locations array bo'sh bo'lmasligi kerak" });
        }

        try
        {
            int savedCount = 0;
            List<string> errors = new();

            foreach (var point in locationDto.Locations)
            {
                try
                {
                    // Parse coordinates
                    if (!double.TryParse(point.Latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out var lat) ||
                        !double.TryParse(point.Longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out var lng))
                    {
                        errors.Add($"Invalid coordinates: Lat={point.Latitude}, Lng={point.Longitude}");
                        continue;
                    }

                    // Parse timestamp
                    if (!DateTime.TryParse(point.Timestamp, out var timestamp))
                    {
                        timestamp = DateTime.UtcNow;
                    }
                    else
                    {
                        timestamp = DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);
                    }

                    // Parse optional fields
                    double? speed = null;
                    if (!string.IsNullOrEmpty(point.Speed))
                    {
                        if (double.TryParse(point.Speed, NumberStyles.Any, CultureInfo.InvariantCulture, out var speedValue))
                            speed = speedValue;
                    }

                    double? accuracy = null;
                    if (!string.IsNullOrEmpty(point.Accuracy))
                    {
                        if (double.TryParse(point.Accuracy, NumberStyles.Any, CultureInfo.InvariantCulture, out var accuracyValue))
                            accuracy = accuracyValue;
                    }

                    // Save to database
                    var result = await _locationService.AddLocationAsync(new Domain.Entities.Location
                    {
                        UserId = locationDto.UserId,
                        Latitude = lat,
                        Longitude = lng,
                        Timestamp = timestamp,
                        Speed = speed,
                        Accuracy = accuracy
                    });

                    if (result != null)
                    {
                        savedCount++;
                    }
                    else
                    {
                        errors.Add($"Failed to save location at {timestamp}");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error: {ex.Message}");
                    _logger.LogError(ex, "Error processing location point");
                }
            }

            return Ok(new
            {
                success = savedCount > 0,
                userId = locationDto.UserId,
                savedCount,
                totalReceived = locationDto.Locations.Count,
                errors = errors.Any() ? errors : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lokatsiyalarni saqlashda xatolik");
            return StatusCode(500, new { message = "Server xatosi", error = ex.Message });
        }
    }

    /// <summary>
    /// GET: api/location/user/{userId}
    /// Get all locations for a specific user
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult> GetUserLocations(int userId, [FromQuery] int limit = 100)
    {
        try
        {
            var locations = await _locationService.GetUserLocationsAsync(userId, limit);

            return Ok(new
            {
                userId,
                count = locations.Count(),
                locations
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lokatsiyalarni olishda xatolik: UserId {UserId}", userId);
            return StatusCode(500, new { message = "Server xatosi" });
        }
    }

    /// <summary>
    /// GET: api/location/latest
    /// Get latest location for each user (for WPF client compatibility)
    /// Returns: List<LocationResponseDto>
    /// </summary>
    [HttpGet("latest")]
    public async Task<ActionResult<List<LocationResponseDto>>> GetLatestLocations()
    {
        try
        {
            var latestLocations = await _locationService.GetLatestLocationsAsync();

            // Convert to LocationResponseDto list (matches WPF client's LocationPoint)
            var response = latestLocations.Select(loc => new LocationResponseDto
            {
                Id = loc.Id,
                UserId = loc.UserId,
                Latitude = loc.Latitude,
                Longitude = loc.Longitude,
                Timestamp = loc.Timestamp, // Already UTC from database
                Speed = loc.Speed,
                Accuracy = loc.Accuracy
            }).ToList();

            _logger.LogInformation("Returning {Count} latest locations", response.Count);

            // Return plain list (not wrapped) for WPF client compatibility
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Oxirgi lokatsiyalarni olishda xatolik");
            return StatusCode(500, new { message = "Server xatosi" });
        }
    }

    /// <summary>
    /// GET: api/location/user/{userId}/date-range?startDate=2025-12-02&endDate=2025-12-03
    /// Get user locations for a specific date range (for Admin panel historical route view)
    /// </summary>
    [HttpGet("user/{userId}/date-range")]
    public async Task<ActionResult> GetUserLocationsByDateRange(
        int userId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            // DateTime Kind ni UTC ga o'zgartirish (PostgreSQL uchun)
            var startDateUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var endDateUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            var locations = await _locationService.GetUserLocationsByDateRangeAsync(userId, startDateUtc, endDateUtc);

            var response = locations.Select(loc => new LocationResponseDto
            {
                Id = loc.Id,
                UserId = loc.UserId,
                Latitude = loc.Latitude,
                Longitude = loc.Longitude,
                Timestamp = loc.Timestamp,
                Speed = loc.Speed,
                Accuracy = loc.Accuracy
            }).ToList();

            _logger.LogInformation(
                "Returning {Count} locations for user {UserId} from {StartDate} to {EndDate}",
                response.Count, userId, startDate, endDate);

            return Ok(new
            {
                userId,
                startDate = startDateUtc,
                endDate = endDateUtc,
                count = response.Count,
                locations = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Date range bo'yicha lokatsiyalarni olishda xatolik");
            return StatusCode(500, new { message = "Server xatosi" });
        }
    }

    /// <summary>
    /// GET: api/location/all-users/date?date=2025-12-02
    /// Get all users' locations for a specific date (Admin overview for one day)
    /// </summary>
    [HttpGet("all-users/date")]
    public async Task<ActionResult> GetAllUsersLocationsByDate([FromQuery] DateTime date)
    {
        try
        {
            // DateTime Kind ni UTC ga o'zgartirish (PostgreSQL uchun)
            var dateUtc = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            var locations = await _locationService.GetAllUsersLocationsByDateAsync(dateUtc);

            // User bo'yicha guruhlash
            var grouped = locations
                .GroupBy(l => l.UserId)
                .Select(g => new
                {
                    userId = g.Key,
                    userName = g.First().User?.Name ?? "Unknown",
                    locationCount = g.Count(),
                    locations = g.Select(loc => new LocationResponseDto
                    {
                        Id = loc.Id,
                        UserId = loc.UserId,
                        Latitude = loc.Latitude,
                        Longitude = loc.Longitude,
                        Timestamp = loc.Timestamp,
                        Speed = loc.Speed,
                        Accuracy = loc.Accuracy
                    }).ToList()
                })
                .ToList();

            _logger.LogInformation(
                "Returning locations for {UserCount} users on date {Date}",
                grouped.Count, dateUtc.ToString("yyyy-MM-dd"));

            return Ok(new
            {
                date = dateUtc.ToString("yyyy-MM-dd"),
                userCount = grouped.Count,
                totalLocations = locations.Count(),
                users = grouped
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Barcha userlar uchun lokatsiyalarni olishda xatolik");
            return StatusCode(500, new { message = "Server xatosi" });
        }
    }
}
