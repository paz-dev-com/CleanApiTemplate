namespace CleanApiTemplate.Core.Entities;

/// <summary>
/// Role entity for authorization
/// </summary>
public class Role : BaseEntity
{
    /// <summary>
    /// Role name (e.g., Admin, User, Manager)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Role description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Normalized role name for case-insensitive comparisons
    /// </summary>
    public string NormalizedName { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to user roles
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}