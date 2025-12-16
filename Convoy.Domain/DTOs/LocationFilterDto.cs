namespace Convoy.Domain.DTOs;

/// <summary>
/// Date range filter for location queries
/// </summary>
public class LocationFilterDto
{
    public int? UserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Response for grouped locations by user and date
/// </summary>
public class UserLocationsGroupedDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int LocationCount { get; set; }
    public List<LocationResponseDto> Locations { get; set; } = new();
}
