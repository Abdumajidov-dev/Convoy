using Convoy.Api.Data;
using Convoy.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Convoy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<LocationController> _logger;

    public LocationController(AppDbContext context, ILogger<LocationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // POST: api/location
    // Array ichida objectlar qabul qilish
    [HttpPost]
    public async Task<ActionResult> PostLocations([FromBody] List<LocationDto>? locations)
    {
        // Kelayotgan raw request body'ni log qilish
        Request.EnableBuffering();
        using (var reader = new StreamReader(Request.Body, leaveOpen: true))
        {
            var body = await reader.ReadToEndAsync();
            Request.Body.Position = 0;
            _logger.LogInformation("Received request body: {Body}", body);
            _logger.LogInformation("Content-Type: {ContentType}", Request.ContentType);
        }

        if (locations == null || !locations.Any())
        {
            _logger.LogWarning("Locations is null or empty");
            return BadRequest(new { message = "Lokatsiyalar bo'sh bo'lmasligi kerak", error = "The locations field is required." });
        }

        return await PostLocationsInternal(locations);
    }

    // POST: api/location/batch
    // Object formatda qabul qilish: { "locations": [...] }
    [HttpPost("batch")]
    public async Task<ActionResult> PostLocationsBatch([FromBody] LocationBatchDto? batchDto)
    {
        // Kelayotgan raw request body'ni log qilish
        Request.EnableBuffering();
        using (var reader = new StreamReader(Request.Body, leaveOpen: true))
        {
            var body = await reader.ReadToEndAsync();
            Request.Body.Position = 0;
            _logger.LogInformation("Batch endpoint - Received request body: {Body}", body);
            _logger.LogInformation("Batch endpoint - Content-Type: {ContentType}", Request.ContentType);
        }

        if (batchDto?.Locations == null || !batchDto.Locations.Any())
        {
            _logger.LogWarning("BatchDto locations is null or empty");
            return BadRequest(new { message = "Lokatsiyalar bo'sh bo'lmasligi kerak", error = "The locations field is required." });
        }

        // Asosiy PostLocations methodini chaqirish - duplicate code oldini olish
        return await PostLocationsInternal(batchDto.Locations);
    }

    private async Task<ActionResult> PostLocationsInternal(List<LocationDto> locations)
    {
        try
        {
            var savedCount = 0;
            var errors = new List<string>();

            foreach (var dto in locations)
            {
                try
                {
                    // String'larni parse qilish
                    if (!double.TryParse(dto.Latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out var latitude))
                    {
                        errors.Add($"UserId {dto.UserId}: Latitude noto'g'ri format");
                        continue;
                    }

                    if (!double.TryParse(dto.Longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out var longitude))
                    {
                        errors.Add($"UserId {dto.UserId}: Longitude noto'g'ri format");
                        continue;
                    }

                    if (!DateTime.TryParse(dto.Timestamp, out var timestamp))
                    {
                        errors.Add($"UserId {dto.UserId}: Timestamp noto'g'ri format");
                        continue;
                    }

                    // UTC ga o'tkazish (PostgreSQL uchun)
                    var utcTimestamp = timestamp.Kind == DateTimeKind.Utc
                        ? timestamp
                        : DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);

                    // User mavjudligini tekshirish
                    var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
                    if (!userExists)
                    {
                        errors.Add($"UserId {dto.UserId} topilmadi");
                        continue;
                    }

                    // Location yaratish
                    var location = new Location
                    {
                        UserId = dto.UserId,
                        Latitude = latitude,
                        Longitude = longitude,
                        Timestamp = utcTimestamp
                    };

                    _context.Locations.Add(location);
                    savedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lokatsiya saqlashda xatolik: UserId {UserId}", dto.UserId);
                    errors.Add($"UserId {dto.UserId}: {ex.Message}");
                }
            }

            // Barcha o'zgarishlarni saqlash
            if (savedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                success = true,
                savedCount = savedCount,
                totalReceived = locations.Count,
                errors = errors.Any() ? errors : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lokatsiyalarni saqlashda xatolik");
            return StatusCode(500, new { message = "Server xatosi", error = ex.Message });
        }
    }

    // GET: api/location/user/{userId}
    // Bitta user uchun barcha lokatsiyalar
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Location>>> GetUserLocations(int userId, [FromQuery] int limit = 100)
    {
        try
        {
            var locations = await _context.Locations
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();

            return Ok(new
            {
                userId = userId,
                count = locations.Count,
                locations = locations
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lokatsiyalarni olishda xatolik: UserId {UserId}", userId);
            return StatusCode(500, new { message = "Server xatosi" });
        }
    }

    // GET: api/location/latest
    // Barcha userlar uchun oxirgi lokatsiyalar
    [HttpGet("latest")]
    public async Task<ActionResult> GetLatestLocations()
    {
        try
        {
            var latestLocations = await _context.Locations
                .Include(l => l.User)
                .GroupBy(l => l.UserId)
                .Select(g => g.OrderByDescending(l => l.Timestamp).FirstOrDefault())
                .Where(l => l != null)
                .ToListAsync();

            return Ok(new
            {
                count = latestLocations.Count,
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
