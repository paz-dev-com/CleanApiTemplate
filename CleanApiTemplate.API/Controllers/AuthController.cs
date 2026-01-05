using CleanApiTemplate.API.Services;
using CleanApiTemplate.Application.Common;
using CleanApiTemplate.Core.Entities;
using CleanApiTemplate.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanApiTemplate.API.Controllers;

/// <summary>
/// Authentication controller
/// Demonstrates JWT token generation and authentication flow
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController(
    IUnitOfWork unitOfWork,
    IJwtTokenService jwtTokenService,
    ICryptographyService cryptographyService,
    ILogger<AuthController> logger) : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IJwtTokenService _jwtTokenService = jwtTokenService;
    private readonly ICryptographyService _cryptographyService = cryptographyService;
    private readonly ILogger<AuthController> _logger = logger;

    /// <summary>
    /// User login endpoint
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JWT token on success</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<LoginResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Login attempt for user: {Username}", request.Username);

        // Find user by username
        var userRepository = _unitOfWork.Repository<User>();
        var users = await userRepository.FindAsync(
            u => u.Username == request.Username && u.IsActive,
            cancellationToken);

        var user = users.FirstOrDefault();

        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found - {Username}", request.Username);
            return Unauthorized(Result<LoginResponse>.Failure("Invalid username or password"));
        }

        // Check if account is locked
        if (user.IsLockedOut)
        {
            _logger.LogWarning("Login failed: Account locked - {Username}", request.Username);
            return Unauthorized(Result<LoginResponse>.Failure("Account is locked. Please try again later."));
        }

        // Verify password
        if (!_cryptographyService.VerifyPassword(request.Password, user.PasswordHash))
        {
            // Increment failed login attempts
            user.AccessFailedCount++;

            // Lock account after 5 failed attempts
            if (user.AccessFailedCount >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(30);
                _logger.LogWarning("Account locked due to failed login attempts: {Username}", request.Username);
            }

            userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogWarning("Login failed: Invalid password - {Username}", request.Username);
            return Unauthorized(Result<LoginResponse>.Failure("Invalid username or password"));
        }

        // Reset failed login count on successful login
        user.AccessFailedCount = 0;
        user.LastLoginAt = DateTime.UtcNow;

        // Load user roles
        var userRoleRepository = _unitOfWork.Repository<UserRole>();
        var userRoles = await userRoleRepository.FindAsync(
            ur => ur.UserId == user.Id,
            cancellationToken);

        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        var roleRepository = _unitOfWork.Repository<Role>();
        var roles = await roleRepository.FindAsync(
            r => roleIds.Contains(r.Id),
            cancellationToken);

        var roleNames = roles.Select(r => r.Name).ToList();

        // Generate JWT token
        var token = _jwtTokenService.GenerateToken(
            user.Id.ToString(),
            user.Username,
            user.Email,
            roleNames);

        // Generate refresh token
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Login successful for user: {Username}", request.Username);

        var response = new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            Username = user.Username,
            Email = user.Email,
            Roles = roleNames
        };

        return Ok(Result<LoginResponse>.Success(response));
    }

    /// <summary>
    /// User registration endpoint
    /// </summary>
    /// <param name="request">Registration data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created user ID</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<Guid>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Registration attempt for user: {Username}", request.Username);

        var userRepository = _unitOfWork.Repository<User>();

        // Check if username already exists
        var usernameExists = await userRepository.AnyAsync(
            u => u.Username == request.Username,
            cancellationToken);

        if (usernameExists)
        {
            return BadRequest(Result<Guid>.Failure("Username already exists"));
        }

        // Check if email already exists
        var emailExists = await userRepository.AnyAsync(
            u => u.Email == request.Email,
            cancellationToken);

        if (emailExists)
        {
            return BadRequest(Result<Guid>.Failure("Email already exists"));
        }

        // Hash password
        var passwordHash = _cryptographyService.HashPassword(request.Password);

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            IsActive = true,
            EmailConfirmed = false,
            PasswordChangedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        await userRepository.AddAsync(user, cancellationToken);

        // Assign default "User" role
        var roleRepository = _unitOfWork.Repository<Role>();
        var userRole = (await roleRepository.FindAsync(
            r => r.NormalizedName == "USER",
            cancellationToken)).FirstOrDefault();

        if (userRole != null)
        {
            var userRoleRepository = _unitOfWork.Repository<UserRole>();
            await userRoleRepository.AddAsync(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoleId = userRole.Id,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = "System",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User registered successfully: {Username}", request.Username);

        return CreatedAtAction(nameof(GetCurrentUser), new { id = user.Id }, Result<Guid>.Success(user.Id));
    }

    /// <summary>
    /// Refresh JWT token using refresh token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New JWT token</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<LoginResponse>>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var principal = _jwtTokenService.ValidateToken(request.Token);
        if (principal == null)
        {
            return BadRequest(Result<LoginResponse>.Failure("Invalid token"));
        }

        var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(Result<LoginResponse>.Failure("Invalid token"));
        }

        var userRepository = _unitOfWork.Repository<User>();
        var user = await userRepository.GetByIdAsync(Guid.Parse(userId), cancellationToken);

        if (user == null || user.RefreshToken != request.RefreshToken ||
            user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return BadRequest(Result<LoginResponse>.Failure("Invalid refresh token"));
        }

        // Load user roles
        var userRoleRepository = _unitOfWork.Repository<UserRole>();
        var userRoles = await userRoleRepository.FindAsync(
            ur => ur.UserId == user.Id,
            cancellationToken);

        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        var roleRepository = _unitOfWork.Repository<Role>();
        var roles = await roleRepository.FindAsync(
            r => roleIds.Contains(r.Id),
            cancellationToken);

        var roleNames = roles.Select(r => r.Name).ToList();

        // Generate new tokens
        var newToken = _jwtTokenService.GenerateToken(
            user.Id.ToString(),
            user.Username,
            user.Email,
            roleNames);

        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new LoginResponse
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            Username = user.Username,
            Email = user.Email,
            Roles = roleNames
        };

        return Ok(Result<LoginResponse>.Success(response));
    }

    /// <summary>
    /// Get current authenticated user information
    /// </summary>
    /// <returns>Current user details</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(Result<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<UserDto>>> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized(Result<UserDto>.Failure("User not authenticated"));
        }

        var userRepository = _unitOfWork.Repository<User>();
        var user = await userRepository.GetByIdAsync(Guid.Parse(userIdClaim), cancellationToken);

        if (user == null)
        {
            return NotFound(Result<UserDto>.Failure("User not found"));
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };

        return Ok(Result<UserDto>.Success(userDto));
    }
}

// DTOs for Auth endpoints
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IEnumerable<string> Roles { get; set; } = new List<string>();
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public class RefreshTokenRequest
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}