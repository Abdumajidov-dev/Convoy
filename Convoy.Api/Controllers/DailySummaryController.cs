using Convoy.Domain.DTOs;
using Convoy.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Convoy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DailySummaryController : ControllerBase
{
    private readonly IDailySummaryService _dailySummaryService;
    private readonly ILogger<DailySummaryController> _logger;

    public DailySummaryController(
        IDailySummaryService dailySummaryService,
        ILogger<DailySummaryController> logger)
    {
        _dailySummaryService = dailySummaryService;
        _logger = logger;
    }

    /// <summary>
    /// User'ning ma'lum kundagi summary'sini olish
    /// </summary>
    [HttpGet("user/{userId}/date/{date}")]
    public async Task<ActionResult<DailySummaryDto>> GetDailySummary(
        int userId,
        DateTime date,
        [FromQuery] bool includeHourlySummaries = true)
    {
        try
        {
            var utcDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            var summary = await _dailySummaryService.GetDailySummaryAsync(userId, utcDate, includeHourlySummaries);

            if (summary == null)
            {
                return NotFound(new { error = $"No summary found for user {userId} on {date:yyyy-MM-dd}" });
            }

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting daily summary for user {userId} on {date:yyyy-MM-dd}");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// User'ning sana oralig'idagi summary'larini olish
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<DailySummaryDto>>> GetUserSummaries(
        int userId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var utcFromDate = fromDate.HasValue ? DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc) : (DateTime?)null;
            var utcToDate = toDate.HasValue ? DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc) : (DateTime?)null;
            var summaries = await _dailySummaryService.GetUserSummariesAsync(userId, utcFromDate, utcToDate);
            return Ok(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting summaries for user {userId}");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Haritada ko'rsatish uchun ma'lum soatlarning summary'sini olish
    /// </summary>
    [HttpPost("hourly")]
    public async Task<ActionResult<List<HourlySummaryDto>>> GetHourlySummaries([FromBody] HourlyFilterDto filter)
    {
        try
        {
            var utcDate = DateTime.SpecifyKind(filter.Date, DateTimeKind.Utc);
            var hourlySummaries = await _dailySummaryService.GetHourlySummariesAsync(
                filter.UserId,
                utcDate,
                filter.Hours
            );

            return Ok(hourlySummaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting hourly summaries for user {filter.UserId} on {filter.Date:yyyy-MM-dd}");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Manual trigger: Ma'lum kun uchun summary yaratish (test uchun)
    /// </summary>
    [HttpPost("create")]
    public async Task<ActionResult<DailySummaryDto>> CreateDailySummary([FromQuery] int userId, [FromQuery] DateTime date)
    {
        try
        {
            var utcDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            var summary = await _dailySummaryService.CreateDailySummaryAsync(userId, utcDate);
            _logger.LogInformation($"Daily summary manually created for user {userId} on {date:yyyy-MM-dd}");
            return Ok(summary);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, $"Cannot create summary for user {userId} on {date:yyyy-MM-dd}");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating daily summary for user {userId} on {date:yyyy-MM-dd}");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Manual trigger: Kechagi kunning barcha ma'lumotlarini process qilish (test uchun)
    /// </summary>
    [HttpPost("process-yesterday")]
    public async Task<ActionResult> ProcessYesterdayData()
    {
        try
        {
            await _dailySummaryService.ProcessYesterdayDataAsync();
            _logger.LogInformation("Yesterday's data processed successfully");
            return Ok(new { message = "Yesterday's data processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing yesterday's data");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Manual trigger: Eski location'larni tozalash (test uchun)
    /// </summary>
    [HttpPost("cleanup")]
    public async Task<ActionResult> CleanupOldLocations([FromQuery] int daysToKeep = 7)
    {
        try
        {
            await _dailySummaryService.CleanupOldLocationsAsync(daysToKeep);
            _logger.LogInformation($"Old locations cleaned up (kept last {daysToKeep} days)");
            return Ok(new { message = $"Old locations cleaned up (kept last {daysToKeep} days)" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old locations");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
