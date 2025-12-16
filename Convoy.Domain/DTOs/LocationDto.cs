namespace Convoy.Domain.DTOs;

// Bitta location point (coordinates va metadata)
public class LocationPointDto
{
    public string Timestamp { get; set; } = string.Empty;
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;
    public string? Speed { get; set; } = string.Empty; // km/h
    public string? Accuracy { get; set; } = string.Empty; // meters
}

// User va uning locationlari (SignalR va REST API uchun)
public class LocationDto
{
    public int UserId { get; set; }
    public List<LocationPointDto> Locations { get; set; } = new();
}
