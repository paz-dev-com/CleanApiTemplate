namespace CleanApiTemplate.Core.Entities;

/// <summary>
/// Many-to-many relationship between Users and Roles
/// </summary>
public class UserRole : BaseEntity
{
    /// <summary>
    /// User identifier
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Role identifier
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Navigation property to User
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Navigation property to Role
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// Date when role was assigned
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who assigned this role
    /// </summary>
    public string AssignedBy { get; set; } = string.Empty;
}