namespace CleanApiTemplate.Core.Entities;

/// <summary>
/// Base entity for all domain entities with common audit fields
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Date and time when the entity was created (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// User who created the entity
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the entity was last updated (UTC)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User who last updated the entity
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Flag for soft delete functionality
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Date and time when the entity was soft deleted (UTC)
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// User who deleted the entity
    /// </summary>
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Row version for optimistic concurrency control
    /// </summary>
    public byte[]? RowVersion { get; set; }
}