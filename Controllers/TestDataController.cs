using Convoy.Domain.DTOs;
using Convoy.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Convoy.Api.Controllers;

[ApiController]
[Route("api/test")]
public class TestDataController : ControllerBase
{
    private readonly IMockDataService _mockDataService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<TestDataController> _logger;

    public TestDataController(
        IMockDataService mockDataService,
        IWebHostEnvironment env,
        ILogger<TestDataController> logger)
    {
        _mockDataService = mockDataService;
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/test/generate-mock-locations
    /// Generate realistic GPS mock data for testing Admin Panel features
    /// Example: Admin wants to see "Where was Employee 1 yesterday?"
    /// </summary>
    [HttpPost("generate-mock-locations")]
    public async Task<ActionResult<MockLocationResponse>> GenerateMockLocations(
        [FromBody] MockLocationRequest request)
    {
        // Faqat Development mode da ruxsat
        if (!_env.IsDevelopment())
        {
            return BadRequest(new { message = "Bu endpoint faqat Development mode da ishlaydi" });
        }

        try
        {
            _logger.LogInformation(
                "Generating mock data for User {UserId} from {StartDate} to {EndDate}",
                request.UserId, request.StartDate, request.EndDate);

            var result = await _mockDataService.GenerateMockLocationsAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mock data generatsiya qilishda xatolik");
            return StatusCode(500, new { message = "Server xatosi", error = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/test/health
    /// Check if test endpoints are available
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "OK",
            message = "Test endpoints are available",
            environment = _env.EnvironmentName,
            timestamp = DateTime.UtcNow
        });
    }
}
