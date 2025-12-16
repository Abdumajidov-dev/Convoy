namespace Convoy.Domain.DTOs;

/// <summary>
/// Response DTO for location data - matches WPF client's LocationPoint model
/// </summary>
public class LocationResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; } // UTC
    public double? Speed { get; set; } // km/h
    public double? Accuracy { get; set; } // meters
}
