namespace CleanApiTemplate.Core.Entities;

/// <summary>
/// Audit log entity for tracking all changes to entities
/// Demonstrates temporal data pattern for audit trail
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>
    /// Name of the entity type that was modified
    /// </summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>
    /// ID of the entity that was modified
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Type of operation performed (Create, Update, Delete)
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Previous state of the entity (JSON serialized)
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// New state of the entity (JSON serialized)
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// List of properties that were changed
    /// </summary>
    public string? ChangedProperties { get; set; }

    /// <summary>
    /// User who performed the action
    /// </summary>
    public string PerformedBy { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the action was performed (UTC)
    /// </summary>
    public DateTime PerformedAt { get; set; }

    /// <summary>
    /// IP address of the user who performed the action
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Additional metadata (JSON serialized)
    /// </summary>
    public string? Metadata { get; set; }
}