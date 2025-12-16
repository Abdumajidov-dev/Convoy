using System.ComponentModel.DataAnnotations.Schema;

namespace Convoy.Domain.Entities;

[Table("branches")]
public class Branch : Auditable
{
    [Column("name")]
    public string? Name { get; set; } = default!;

    [Column("code")]
    public string? Code { get; set; } = default!;

    [Column("state_id")]
    public int? StateId { get; set; }

    [Column("state_name")]
    public string? StateName { get; set; } = default!;

    [Column("region_id")]
    public int? RegionId { get; set; }

    [Column("region_name")]
    public string? RegionName { get; set; } = default!;

    [Column("address")]
    public string? Address { get; set; } = default!;

    [Column("phone_number")]
    public string? PhoneNumber { get; set; } = default!;

    [Column("target")]
    public string? Target { get; set; } = default!;

    [Column("location")]
    public string? LocationUrl { get; set; } = default!;

    [Column("responsible_worker")]
    public string? ResponsibleWorker { get; set; } = default!;
}
