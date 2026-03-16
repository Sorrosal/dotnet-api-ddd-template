namespace DotnetApiDddTemplate.Domain.Common;

/// <summary>
/// Base class for auditable entities.
/// Extends BaseEntity with soft delete and auditing fields.
/// </summary>
public abstract class AuditableEntity<TId> : BaseEntity<TId>
    where TId : struct
{
    /// <summary>
    /// UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// User who created the entity.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// UTC last modification timestamp.
    /// </summary>
    public DateTime? ModifiedAtUtc { get; set; }

    /// <summary>
    /// User who last modified the entity.
    /// </summary>
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Soft delete timestamp. Null if not deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Is entity deleted (soft delete).
    /// </summary>
    public bool IsDeleted => DeletedAt.HasValue;

    /// <summary>
    /// Row version for optimistic concurrency control.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];
}
