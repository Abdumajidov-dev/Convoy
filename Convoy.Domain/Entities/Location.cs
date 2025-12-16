using System.ComponentModel.DataAnnotations.Schema;

namespace Convoy.Domain.Entities;

[Table("locations")]
public class Location : Auditable
{
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("latitude")]
    public double Latitude { get; set; }

    [Column("longitude")]
    public double Longitude { get; set; }

    [Column("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Column("speed")]
    public double? Speed { get; set; }

    [Column("accuracy")]
    public double? Accuracy { get; set; }

    // Navigation property
    public User User { get; set; } = null!;
}
