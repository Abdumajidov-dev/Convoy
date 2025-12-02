namespace Convoy.Api.Models;

// Mijozdan kelayotgan bitta lokatsiya
public class LocationDto
{
    public int UserId { get; set; }
    public string Timestamp { get; set; } = string.Empty; // Vaqt string formatda keladi
    public string Latitude { get; set; } = string.Empty; // Latitude string formatda
    public string Longitude { get; set; } = string.Empty; // Longitude string formatda
}

// Array ichida kelayotgan lokatsiyalar uchun
public class LocationBatchDto
{
    public List<LocationDto> Locations { get; set; } = new();
}
