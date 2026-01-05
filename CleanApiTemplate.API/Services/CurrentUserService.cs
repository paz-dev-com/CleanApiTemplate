using CleanApiTemplate.Core.Interfaces;
using System.Security.Claims;

namespace CleanApiTemplate.API.Services;

/// <summary>
/// Implementation of ICurrentUserService
/// Extracts user information from HTTP context and JWT claims
/// </summary>
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
        _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");

    public string? Username =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name) ??
        _httpContextAccessor.HttpContext?.User?.FindFirstValue("preferred_username");

    public string? Email =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public IEnumerable<string> Roles
    {
        get
        {
            var claims = _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role);
            return claims?.Select(c => c.Value) ?? Enumerable.Empty<string>();
        }
    }

    public bool HasRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }

    public string? IpAddress =>
        _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
}