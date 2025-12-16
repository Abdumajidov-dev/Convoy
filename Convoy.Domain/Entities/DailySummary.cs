using System.ComponentModel.DataAnnotations.Schema;

namespace Convoy.Domain.Entities;

/// <summary>
/// Kunlik umumiy statistika (har kecha soat 12:00 da yaratiladi)
/// </summary>
[Table("daily_summaries")]
public class DailySummary : Auditable
{
    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>
    /// Sana (faqat kun, soatsiz: 2024-12-09 00:00:00)
    /// </summary>
    [Column("date")]
    public DateTime Date { get; set; }

    /// <summary>
    /// Umumiy location'lar soni
    /// </summary>
    [Column("total_locations")]
    public int TotalLocations { get; set; }

    /// <summary>
    /// Birinchi location vaqti
    /// </summary>
    [Column("first_location_time")]
    public DateTime? FirstLocationTime { get; set; }

    /// <summary>
    /// Oxirgi location vaqti
    /// </summary>
    [Column("last_location_time")]
    public DateTime? LastLocationTime { get; set; }

    /// <summary>
    /// Jami bosib o'tgan masofa (km)
    /// </summary>
    [Column("total_distance_km")]
    public double TotalDistanceKm { get; set; }

    /// <summary>
    /// O'rtacha tezlik (km/h)
    /// </summary>
    [Column("average_speed")]
    public double? AverageSpeed { get; set; }

    /// <summary>
    /// Maksimal tezlik (km/h)
    /// </summary>
    [Column("max_speed")]
    public double? MaxSpeed { get; set; }

    /// <summary>
    /// Geografik chegara - minimum latitude
    /// </summary>
    [Column("min_latitude")]
    public double? MinLatitude { get; set; }

    /// <summary>
    /// Geografik chegara - maksimal latitude
    /// </summary>
    [Column("max_latitude")]
    public double? MaxLatitude { get; set; }

    /// <summary>
    /// Geografik chegara - minimum longitude
    /// </summary>
    [Column("min_longitude")]
    public double? MinLongitude { get; set; }

    /// <summary>
    /// Geografik chegara - maksimal longitude
    /// </summary>
    [Column("max_longitude")]
    public double? MaxLongitude { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<HourlySummary> HourlySummaries { get; set; } = new List<HourlySummary>();
}
