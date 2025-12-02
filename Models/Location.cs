namespace Convoy.Api.Models;

public class Location
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public double? Speed { get; set; } // Tezlik (optional)
    public double? Accuracy { get; set; } // Aniqlik (optional)

    // Navigation property
    public User User { get; set; } = null!;
}
