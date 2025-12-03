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
    /// Get latest location for all users
    /// </summary>
    [HttpGet("latest")]
    public async Task<ActionResult> GetLatestLocations()
    {
        try
        {
            var latestLocations = await _locationService.GetLatestLocationsAsync();

            return Ok(new
            {
                count = latestLocations.Count(),
                locations = latestLocations
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Oxirgi lokatsiyalarni olishda xatolik");
            return StatusCode(500, new { message = "Server xatosi" });
        }
    }
}
