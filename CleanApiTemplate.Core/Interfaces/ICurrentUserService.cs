namespace CleanApiTemplate.Core.Interfaces;

/// <summary>
/// Interface for current user context access
/// Used for retrieving authenticated user information
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Get the current authenticated user's ID
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Get the current authenticated user's username
    /// </summary>
    string? Username { get; }

    /// <summary>
    /// Get the current authenticated user's email
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Check if user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Get the current user's roles
    /// </summary>
    IEnumerable<string> Roles { get; }

    /// <summary>
    /// Check if user has a specific role
    /// </summary>
    /// <param name="role">Role name to check</param>
    /// <returns>True if user has the role, false otherwise</returns>
    bool HasRole(string role);

    /// <summary>
    /// Get user's IP address
    /// </summary>
    string? IpAddress { get; }
}