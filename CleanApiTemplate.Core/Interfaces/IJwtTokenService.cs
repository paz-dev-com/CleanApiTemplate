using System.Security.Claims;

namespace CleanApiTemplate.Core.Interfaces;

/// <summary>
/// Interface for JWT token management
/// Abstracts token generation and validation
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generate JWT token for a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="username">Username</param>
    /// <param name="email">User email</param>
    /// <param name="roles">User roles</param>
    /// <returns>JWT token string</returns>
    string GenerateToken(string userId, string username, string email, IEnumerable<string> roles);

    /// <summary>
    /// Generate refresh token
    /// </summary>
    /// <returns>Refresh token string</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Validate JWT token and extract claims
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>ClaimsPrincipal if valid, null otherwise</returns>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Extract user ID from token
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>User ID or null</returns>
    string? GetUserIdFromToken(string token);
}
