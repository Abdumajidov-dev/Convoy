using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Convoy.Domain.Entities;

/// <summary>
/// Base class for all auditable entities
/// Provides common audit fields: Id, CreatedAt, UpdatedAt, DeletedAt, IsDeleted
/// </summary>
public abstract class Auditable
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;
}
