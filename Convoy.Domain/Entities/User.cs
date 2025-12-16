using System.ComponentModel.DataAnnotations.Schema;

namespace Convoy.Domain.Entities;

[Table("users")]
public class User : Auditable
{
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("phone")]
    public string Phone { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    // Relationships
    public ICollection<Location> Locations { get; set; } = new List<Location>();
    public ICollection<DailySummary> DailySummaries { get; set; } = new List<DailySummary>();
    public ICollection<HourlySummary> HourlySummaries { get; set; } = new List<HourlySummary>();
}
