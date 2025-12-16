using System.ComponentModel.DataAnnotations.Schema;

namespace Convoy.Domain.Entities;

/// <summary>
/// Soatlik statistika (soat bo'yicha filter qilish uchun)
/// Haritada ma'lum soat oralig'ini ko'rsatish uchun kerak
/// </summary>
[Table("hourly_summaries")]
public class HourlySummary : Auditable
{
    [Column("daily_summary_id")]
    public int DailySummaryId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>
    /// Soat (0-23)
    /// </summary>
    [Column("hour")]
    public int Hour { get; set; }

    /// <summary>
    /// Shu soatda jo'natilgan location'lar soni
    /// </summary>
    [Column("location_count")]
    public int LocationCount { get; set; }

    /// <summary>
    /// Shu soatda bosib o'tgan masofa (km)
    /// </summary>
    [Column("distance_km")]
    public double DistanceKm { get; set; }

    /// <summary>
    /// O'rtacha tezlik (km/h)
    /// </summary>
    [Column("average_speed")]
    public double? AverageSpeed { get; set; }

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
    public DailySummary DailySummary { get; set; } = null!;
    public User User { get; set; } = null!;
}
