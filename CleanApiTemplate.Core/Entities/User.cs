namespace CleanApiTemplate.Core.Entities;

/// <summary>
/// User entity for authentication and authorization
/// Demonstrates user management with roles and security
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Unique username for login
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password (never store plain text passwords!)
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// User's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Phone number (optional)
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Flag indicating if user account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Flag indicating if email is verified
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Flag indicating if phone is verified
    /// </summary>
    public bool PhoneNumberConfirmed { get; set; }

    /// <summary>
    /// Flag for two-factor authentication
    /// </summary>
    public bool TwoFactorEnabled { get; set; }

    /// <summary>
    /// Number of failed login attempts (for account lockout)
    /// </summary>
    public int AccessFailedCount { get; set; }

    /// <summary>
    /// Date when account lockout ends (if locked)
    /// </summary>
    public DateTime? LockoutEnd { get; set; }

    /// <summary>
    /// Last login date and time
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Last password change date
    /// </summary>
    public DateTime? PasswordChangedAt { get; set; }

    /// <summary>
    /// Refresh token for JWT authentication
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Refresh token expiration date
    /// </summary>
    public DateTime? RefreshTokenExpiryTime { get; set; }

    /// <summary>
    /// Navigation property to user roles
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    /// <summary>
    /// Full name computed property
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Check if account is locked out
    /// </summary>
    public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
}
