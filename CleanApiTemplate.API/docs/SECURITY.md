# Security Best Practices

## Table of Contents
1. [Authentication & Authorization](#authentication--authorization)
2. [Secret Management](#secret-management)
3. [Encryption & Hashing](#encryption--hashing)
4. [API Security](#api-security)
5. [Database Security](#database-security)
6. [Clean Architecture Security Pattern](#clean-architecture-security-pattern)

## Clean Architecture Security Pattern

This template follows Clean Architecture principles for security implementation across **5 layers**:

### Service Layer Organization

**? Core Layer (Domain)**
- Contains **only interfaces** (zero dependencies)
- `IJwtTokenService` - Interface for JWT operations
- `ICryptographyService` - Interface for encryption/hashing
- `ISecretManager` - Interface for secret management
- `ICurrentUserService` - Interface for current user access

**? Application Layer (Use Cases)**
- Contains **business logic** and **CQRS handlers**
- Uses interfaces from Core layer
- Zero infrastructure dependencies

**? Infrastructure Layer (External Services)**
- `JwtTokenService` - JWT token generation and validation
- `CryptographyService` - Encryption, hashing, password management
- `AzureKeyVaultSecretManager` - Secret retrieval from Azure KeyVault
- `SystemInteropService` - Windows Registry, PowerShell, Services
- `HealthCheckService` - Database connectivity checks

**? Data Layer (Persistence)**
- `ApplicationDbContext` - EF Core database context
- `Repository<T>` and `UnitOfWork` - Data access implementations

**? API Layer (Presentation)**
- `CurrentUserService` - Extracts user info from HttpContext (JWT claims)
- Controllers - HTTP endpoints

### Dependency Flow
```
API Controllers ? Application Handlers ? Core Interfaces ? Infrastructure Implementations
                                      ?
                                   Domain
```

This ensures:
- Core layer has zero infrastructure dependencies
- Application layer only depends on Core abstractions
- All security services are mockable for testing
- Implementations can be swapped without changing business logic

## Authentication & Authorization

### JWT (JSON Web Tokens)

#### Service Implementation
The `JwtTokenService` is implemented in the **Infrastructure layer** (`CleanApiTemplate.Infrastructure/Services/JwtTokenService.cs`) as it's an external service concern:

```csharp
// Core/Interfaces/IJwtTokenService.cs - Interface defined in Core
public interface IJwtTokenService
{
    string GenerateToken(string userId, string username, string email, IEnumerable<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    string? GetUserIdFromToken(string token);
}

// Infrastructure/Services/JwtTokenService.cs - Implementation in Infrastructure layer
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    
    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    // Implementation details with System.IdentityModel.Tokens.Jwt...
}

// Registered in Infrastructure/DependencyInjection.cs
services.AddScoped<IJwtTokenService, JwtTokenService>();
```

#### Token Structure
```
Header.Payload.Signature
eyJhbGc...eyJzdWI...SflKxwRJ
```

#### Configuration
```csharp
// appsettings.json
{
  "JwtSettings": {
    "SecretKey": "YourSecure32CharacterKeyHere!!!",
    "Issuer": "CleanApiTemplate",
    "Audience": "CleanApiTemplateUsers",
    "ExpirationInMinutes": "60"
  }
}

// Program.cs
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"])),
            ClockSkew = TimeSpan.Zero
        };
    });
```

**Important**: JWT secret must be at least 32 characters for HS256 algorithm.

#### Generating Tokens
```csharp
// In AuthController or command handler
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Result<LoginResponse>> Handle(...)
    {
        // Validate credentials...
        
        var token = _jwtTokenService.GenerateToken(
            user.Id.ToString(),
            user.Username,
            user.Email,
            user.Roles.Select(r => r.Name)
        );
        
        return Result<LoginResponse>.Success(new LoginResponse 
        { 
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        });
    }
}
```

#### Using in Controllers
```csharp
// Example: Products Controller with explicit role-based authorization

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    
    // Public endpoints - No authentication required
    [HttpGet]
    [AllowAnonymous] // Explicitly allow anonymous access for browsing
    public async Task<IActionResult> GetProducts() 
    { 
        // Anyone can browse products
    }

    [HttpGet("{id}")]
    [AllowAnonymous] // Public product details
    public async Task<IActionResult> GetProduct(Guid id) { }

    // Authenticated endpoints - Requires User or Admin role
    [HttpPost]
    [Authorize(Roles = "Admin")] // Multiple roles accepted (OR logic)
    public async Task<IActionResult> CreateProduct() 
    { 
        // Both regular users and admins can create
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")] // Multiple roles accepted
    public async Task<IActionResult> UpdateProduct(Guid id) 
    { 
        // Both regular users and admins can update
    }

    // Admin-only endpoints
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Only Admin role allowed
    public async Task<IActionResult> DeleteProduct(Guid id) 
    { 
        // Only administrators can delete
    }

    // Custom policy-based authorization
    [HttpPost("sensitive")]
    [Authorize(Policy = "MinimumAge")] // Requires custom policy
    public async Task<IActionResult> SensitiveOperation() { }
}

// Auth Controller - Demonstrates different authentication requirements
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // Anonymous endpoints
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login() { }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register() { }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken() { }

    // Authenticated endpoint - Any logged-in user
    [HttpGet("me")]
    [Authorize] // No role specified = any authenticated user
    public async Task<IActionResult> GetCurrentUser() 
    { 
        // Available to all authenticated users
    }
}
```

### Role-Based Authorization Summary

| Authorization Attribute | Description | Use Case |
|------------------------|-------------|----------|
| `[AllowAnonymous]` | No authentication required | Public endpoints (browsing, login, register) |
| `[Authorize]` | Any authenticated user | Get current user info, basic protected resources |
| `[Authorize(Roles = "Admin")]` | Multiple roles (OR logic) | Create/Update operations for regular users and admins |
| `[Authorize(Roles = "Admin")]` | Single role required | Administrative operations (delete, manage users) |
| `[Authorize(Policy = "PolicyName")]` | Custom policy | Complex authorization logic (age verification, ownership) |

### Best Practices

1. **Be Explicit**: Always use `[AllowAnonymous]` or `[Authorize]` - don't rely on defaults
2. **Use Role Constraints**: Specify roles when possible: `[Authorize(Roles = "Admin")]`
3. **Multiple Roles**: Use comma-separated list for OR logic: `[Authorize(Roles = "Admin")]`
4. **Custom Policies**: For complex logic, create custom authorization policies
5. **Least Privilege**: Grant minimal permissions necessary for each role
6. **Document Endpoints**: Clearly indicate which roles can access each endpoint

### Current User Service

The `CurrentUserService` is in the **API layer** because it depends on `IHttpContextAccessor`:

```csharp
// Core/Interfaces/ICurrentUserService.cs - Interface in Core
public interface ICurrentUserService
{
    string? UserId { get; }
    string? Username { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
    bool HasRole(string role);
}

// API/Services/CurrentUserService.cs - Implementation in API layer
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        
    public string? Username =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
        
    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        
    public IEnumerable<string> Roles { get; }
    
    public bool HasRole(string role) { }
}

// Registered in API/Program.cs
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
```

### OAuth 2.0 / OpenID Connect

```csharp
services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options =>
{
    options.Authority = "https://your-identity-provider.com";
    options.ClientId = "your-client-id";
    options.ClientSecret = "your-client-secret";
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
});
```

### Authorization Policies

```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("MinimumAge", policy =>
        policy.Requirements.Add(new MinimumAgeRequirement(18)));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("SameUser", policy =>
        policy.Requirements.Add(new SameUserRequirement()));
});
```

## Secret Management

### User Secrets (Development)

```bash
# Initialize user secrets
dotnet user-secrets init --project CleanApiTemplate.API

# Set a secret
dotnet user-secrets set "JwtSettings:SecretKey" "YourSecretKeyMinimumLength32Characters!"

# List secrets
dotnet user-secrets list
```

### Azure KeyVault (Production)

#### Setup
```csharp
// Core/Interfaces/ISecretManager.cs - Interface in Core
public interface ISecretManager
{
    Task<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default);
}

// Infrastructure/Services/AzureKeyVaultSecretManager.cs - Implementation in Infrastructure
public class AzureKeyVaultSecretManager : ISecretManager
{
    private readonly SecretClient _secretClient;
    // Azure SDK implementation

    public AzureKeyVaultSecretManager(IConfiguration configuration)
    {
        var keyVaultUrl = configuration["AzureKeyVault:VaultUri"];
        _secretClient = new SecretClient(
            new Uri(keyVaultUrl),
            new DefaultAzureCredential()
        );
    }

    public async Task<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var secret = await _secretClient.GetSecretAsync(key, cancellationToken);
            return secret.Value.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }
}

// Registered in Infrastructure/DependencyInjection.cs
services.AddSingleton<ISecretManager, AzureKeyVaultSecretManager>();
```

### Environment Variables

```csharp
// Set in environment
// Windows: set ConnectionStrings__DefaultConnection=...
// Linux: export ConnectionStrings__DefaultConnection=...

// Access in code
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
```

## Encryption & Hashing

### Password Hashing

The `CryptographyService` is in the **Infrastructure layer**:

#### PBKDF2 (Recommended)
```csharp
// Core/Interfaces/ICryptographyService.cs - Interface in Core
public interface ICryptographyService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string storedHash);
    string Encrypt(string plainText);
    string Decrypt(string encryptedText);
    string ComputeSha256Hash(string input);
}

// Infrastructure/Services/CryptographyService.cs - Implementation
public class CryptographyService : ICryptographyService
{
    public string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            iterations: 10000,
            HashAlgorithmName.SHA256
        );
        byte[] hash = pbkdf2.GetBytes(32);

        byte[] hashBytes = new byte[48];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 32);

        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string storedHash)
    {
        byte[] hashBytes = Convert.FromBase64String(storedHash);
        byte[] salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);

        var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            iterations: 10000,
            HashAlgorithmName.SHA256
        );
        byte[] hash = pbkdf2.GetBytes(32);

        for (int i = 0; i < 32; i++)
        {
            if (hashBytes[i + 16] != hash[i])
                return false;
        }
        return true;
    }
    
    // AES-256 Encryption implementation...
    // SHA-256 Hashing implementation...
}

// Registered in Infrastructure/DependencyInjection.cs
services.AddSingleton<ICryptographyService, CryptographyService>();
```

### Data Encryption

#### AES-256 Encryption
```csharp
public string Encrypt(string plainText, string key)
{
    using var aes = Aes.Create();
    aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32));
    aes.GenerateIV();

    using var encryptor = aes.CreateEncryptor();
    var plainBytes = Encoding.UTF8.GetBytes(plainText);
    var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

    // Prepend IV to encrypted data
    var result = new byte[aes.IV.Length + encryptedBytes.Length];
    Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
    Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

    return Convert.ToBase64String(result);
}

public string Decrypt(string encryptedText, string key)
{
    var fullBytes = Convert.FromBase64String(encryptedText);

    using var aes = Aes.Create();
    aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32));

    // Extract IV
    var iv = new byte[aes.IV.Length];
    var encryptedBytes = new byte[fullBytes.Length - iv.Length];
    Buffer.BlockCopy(fullBytes, 0, iv, 0, iv.Length);
    Buffer.BlockCopy(fullBytes, iv.Length, encryptedBytes, 0, encryptedBytes.Length);
    aes.IV = iv;

    using var decryptor = aes.CreateDecryptor();
    var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

    return Encoding.UTF8.GetString(decryptedBytes);
}
```

### Hashing

#### SHA-256 (For Data Integrity)
```csharp
public string ComputeSha256Hash(string input)
{
    using var sha256 = SHA256.Create();
    var bytes = Encoding.UTF8.GetBytes(input);
    var hashBytes = sha256.ComputeHash(bytes);
    return Convert.ToHexString(hashBytes).ToLower();
}
```

## API Security

### HTTPS Enforcement

```csharp
// Program.cs
app.UseHttpsRedirection();

// Force HTTPS in production
if (app.Environment.IsProduction())
{
    app.UseHsts();
}
```

### CORS Configuration

```csharp
// Restrictive CORS (Recommended)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Authorization", "Content-Type")
              .AllowCredentials();
    });
});

// Use in production
app.UseCors("Production");
```

### Rate Limiting

```csharp
// Install: AspNetCoreRateLimit
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Limit = 100,
            Period = "1m"
        }
    };
});
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

app.UseIpRateLimiting();
```

### Input Validation

```csharp
// FluentValidation
public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"[A-Z]").WithMessage("Must contain uppercase")
            .Matches(@"[a-z]").WithMessage("Must contain lowercase")
            .Matches(@"[0-9]").WithMessage("Must contain number")
            .Matches(@"[\W_]").WithMessage("Must contain special character");
    }
}
```

### SQL Injection Prevention

```csharp
// NEVER do this
var sql = $"SELECT * FROM Users WHERE Username = '{username}'";

// Always use parameterized queries
var user = await context.Users
    .FromSqlRaw("SELECT * FROM Users WHERE Username = {0}", username)
    .FirstOrDefaultAsync();

// Or use LINQ (automatically safe)
var user = await context.Users
    .FirstOrDefaultAsync(u => u.Username == username);
```

### XSS Prevention

```csharp
// Encode output
using System.Web;
var safeOutput = HttpUtility.HtmlEncode(userInput);

// Validate input
RuleFor(x => x.Description)
    .Must(BeValidHtml)
    .WithMessage("Invalid HTML content");

private bool BeValidHtml(string html)
{
    // Use HTML sanitizer library
    var sanitizer = new HtmlSanitizer();
    var clean = sanitizer.Sanitize(html);
    return clean == html;
}
```

## Database Security

### Connection String Security

```csharp
// Use integrated security when possible
"Server=localhost;Database=MyDb;Integrated Security=true;"

// Encrypt connection strings
// In production, store in Azure KeyVault or AWS Secrets Manager

// Use connection string encryption in config
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    var connectionString = _secretManager.GetSecretAsync("DbConnectionString").Result;
    optionsBuilder.UseSqlServer(connectionString);
}
```

### Least Privilege Principle

```sql
-- Create dedicated application user
CREATE LOGIN AppUser WITH PASSWORD = 'SecurePassword123!';
CREATE USER AppUser FOR LOGIN AppUser;

-- Grant only necessary permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.Products TO AppUser;
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.Categories TO AppUser;

-- Don't grant these
-- REVOKE CREATE TABLE TO AppUser;
-- REVOKE ALTER TO AppUser;
-- REVOKE DROP TO AppUser;
```

### Audit Logging

```csharp
public class AuditLog
{
    public Guid Id { get; set; }
    public string EntityName { get; set; }
    public string Action { get; set; }
    public string PerformedBy { get; set; }
    public DateTime PerformedAt { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
}

// Implement in DbContext
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
{
    var auditEntries = OnBeforeSaveChanges();
    var result = await base.SaveChangesAsync(cancellationToken);
    await OnAfterSaveChanges(auditEntries);
    return result;
}
```

## Security Checklist

### Development
- [ ] Use User Secrets for local development
- [ ] Enable HTTPS with valid certificates
- [ ] Implement proper error handling (don't expose stack traces)
- [ ] Use parameterized queries
- [ ] Validate all inputs
- [ ] Implement rate limiting

### Production
- [ ] Use Azure KeyVault or similar for secrets
- [ ] Enable HSTS
- [ ] Configure restrictive CORS
- [ ] Use strong JWT secret keys
- [ ] Implement proper logging and monitoring
- [ ] Use connection string encryption
- [ ] Enable SQL Server TDE (Transparent Data Encryption)
- [ ] Implement database backups
- [ ] Use least privilege for database accounts
- [ ] Enable audit logging
- [ ] Keep all packages updated
- [ ] Implement IP whitelisting if applicable
- [ ] Use API gateways for additional security layer
- [ ] Implement proper session management
- [ ] Use secure cookies (HttpOnly, Secure, SameSite)

## Clean Architecture Benefits for Security

### Testability
All security services can be easily mocked:

```csharp
// In unit tests
var mockJwtService = new Mock<IJwtTokenService>();
mockJwtService.Setup(x => x.GenerateToken(It.IsAny<string>(), ...))
    .Returns("mock-jwt-token");

var mockCrypto = new Mock<ICryptographyService>();
mockCrypto.Setup(x => x.HashPassword(It.IsAny<string>()))
    .Returns("hashed-password");

var mockSecretManager = new Mock<ISecretManager>();
mockSecretManager.Setup(x => x.GetSecretAsync("key", default))
    .ReturnsAsync("secret-value");
```

### Swappable Implementations
You can easily switch between different implementations:

```csharp
// Development: Use simple JWT service
services.AddScoped<IJwtTokenService, JwtTokenService>();

// Production: Use Azure AD B2C (custom implementation)
services.AddScoped<IJwtTokenService, AzureAdB2CTokenService>();

// Testing: Use mock implementation
services.AddScoped<IJwtTokenService, MockJwtTokenService>();
```

### Zero Core Dependencies
The Core layer (Domain + Interfaces) has **zero infrastructure dependencies**:
- No `System.IdentityModel.Tokens.Jwt`
- No `Azure.Security.KeyVault`
- No `Microsoft.EntityFrameworkCore`
- Only pure .NET interfaces

This means your domain and business logic never depend on external infrastructure.
