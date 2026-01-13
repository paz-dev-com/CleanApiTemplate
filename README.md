# Clean API Template - .NET 10

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean-blue)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![Code Quality](https://img.shields.io/badge/Code%20Quality-EditorConfig-success)](https://editorconfig.org/)

A production-ready, enterprise-grade Clean Architecture API template demonstrating modern .NET 10 development practices, CQRS patterns, security implementation, database optimization, code quality enforcement, and strict adherence to Clean Architecture principles.

This template **strictly adheres** to Clean Architecture principles:
- **Pure Domain Layer (Core)** - Zero dependencies, only domain entities and interfaces
- **Pure Application Layer** - Only MediatR, FluentValidation, and logging abstractions
- **Proper Dependency Direction** - Dependencies flow inward only (API/Infrastructure/Data → Application → Core)
- **Interface-Based Abstractions** - All services program to interfaces
- **High Testability** - 100% mockable components
- **Infrastructure Independence** - Swap implementations without changing business logic
- **Code Quality Enforcement** - EditorConfig, analyzers, and formatting rules

## ➡️ Architecture Overview

This template implements **Clean Architecture** with clear separation of concerns across **5 layers**:

```
CleanApiTemplate/
├── CleanApiTemplate.API/                # Presentation Layer (Web API)
│   ├── Controllers/                     # HTTP endpoints
│   ├── Middleware/                      # Cross-cutting concerns
│   └── Services/                        # Presentation-specific services (CurrentUserService)
│
├── CleanApiTemplate.Core/               # Domain Layer (ZERO dependencies)
│   ├── Entities/                        # Domain entities (Product, User, Category, etc.)
│   └── Interfaces/                      # Abstractions for all external dependencies
│       ├── IRepository.cs
│       ├── IUnitOfWork.cs
│       ├── IJwtTokenService.cs
│       ├── ICryptographyService.cs
│       ├── ISecretManager.cs
│       ├── ISystemInteropService.cs
│       ├── IHealthCheckService.cs
│       └── ICurrentUserService.cs
│
├── CleanApiTemplate.Application/        # Application Layer (Use Cases)
│   ├── Features/                        # CQRS Commands & Queries
│   │   └── Products/
│   │       ├── Commands/                # CreateProductCommand, UpdateProductCommand
│   │       └── Queries/                 # GetProductsQuery, GetProductByIdQuery
│   ├── Behaviors/                       # MediatR pipeline behaviors
│   │   ├── ValidationBehavior.cs        # FluentValidation integration
│   │   ├── PerformanceBehavior.cs       # Performance monitoring
│   │   └── TransactionBehavior.cs       # Transaction management
│   ├── Common/                          # Shared DTOs and base classes
│   │   ├── Result.cs
│   │   ├── PaginatedResult.cs
│   │   ├── CommandBase.cs
│   │   └── QueryBase.cs
│   └── [Dependencies: MediatR, FluentValidation, Logging.Abstractions only]
│
├── CleanApiTemplate.Infrastructure/     # Infrastructure Layer (External Services)
│   └── Services/                        # Infrastructure service implementations
│       ├── JwtTokenService.cs           # JWT token generation
│       ├── CryptographyService.cs       # Encryption, hashing, password management
│       ├── AzureKeyVaultSecretManager.cs # Azure KeyVault integration
│       ├── SystemInteropService.cs      # Windows Registry, PowerShell, Services
│       └── HealthCheckService.cs        # Health check implementations
│
└── CleanApiTemplate.Data/               # Data Access Layer (Persistence)
    ├── Persistence/                     # EF Core DbContext
    │   ├── ApplicationDbContext.cs
    │   └── Configurations/              # Entity configurations
    └── Repositories/                    # Data access implementations
        ├── Repository.cs
        └── UnitOfWork.cs
```

### Layer Responsibilities

#### **Core Layer** (Domain) - Pure C# ✅
- **Domain Entities**: Business objects with validation rules (Product, User, Category, etc.)
- **Interfaces**: Abstractions for ALL external dependencies
- **Zero Dependencies**: No NuGet packages, only pure .NET types
- **Framework Agnostic**: Can be used in any .NET application

#### **Application Layer** (Use Cases) - Infrastructure-Free ✅
- **CQRS Patterns**: Commands and Queries with MediatR
- **Business Logic**: Validators, handlers, and orchestration
- **Pipeline Behaviors**: Validation, Performance monitoring, Transaction management
- **DTOs**: Data transfer objects for API responses
- **Minimal Dependencies**: Only MediatR (14.0.0), FluentValidation (12.1.1), and Logging abstractions (10.0.0)
- **NO** Entity Framework ✅
- **NO** Azure libraries ✅
- **NO** infrastructure dependencies ✅

#### **Infrastructure Layer** (External Services) ✅
- **JWT Token Service**: Token generation and validation
- **Cryptography Service**: AES encryption, password hashing, SHA-256
- **Azure KeyVault**: Secret management
- **System Interop**: Windows Registry, PowerShell, Windows Services
- **Health Checks**: Database, external service health monitoring
- **Dependencies**: Azure SDK, System.Management.Automation, JWT libraries

#### **Data Layer** (Persistence) ✅
- **Database Context**: EF Core configuration
- **Repositories**: Generic repository pattern implementation
- **Unit of Work**: Transaction coordination
- **Entity Configurations**: Fluent API mappings
- **Database Migrations**: EF Core migrations
- **Dependencies**: EF Core, Dapper (for raw SQL)

#### **API Layer** (Presentation) ✅
- **Controllers**: HTTP endpoints (ProductsController, AuthController)
- **Middleware**: Exception handling and cross-cutting concerns
- **Presentation Services**: CurrentUserService (depends on HttpContext)
- **Configuration**: DI registration, authentication, logging setup
- **Swagger/OpenAPI**: API documentation

## ➡️ Core Concepts Demonstrated

### 1. Modern .NET 10 Features

#### Dependency Injection (DI)
```csharp
// Core layer defines interfaces
public interface IJwtTokenService { ... }
public interface ICryptographyService { ... }

// Infrastructure layer implements services
public class JwtTokenService : IJwtTokenService { ... }
public class CryptographyService : ICryptographyService { ... }

// API layer registers services
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddDataServices(builder.Configuration);
builder.Services.AddApplicationServices();

// All services are registered via interfaces for testability
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<ICryptographyService, CryptographyService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
```

#### Configuration Management
```csharp
// Strongly-typed configuration sections
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
```

#### Structured Logging with Serilog
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

### 2. Clean Architecture Patterns

#### CQRS (Command Query Responsibility Segregation)
Separates read and write operations for better scalability:

**Commands** (Modify State):
```csharp
// Application/Features/Products/Commands/CreateProductCommand.cs
public class CreateProductCommand : CommandBase<Result<Guid>>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
}

// Handler depends on abstractions only (defined in Core)
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    
    // Fully testable - all dependencies are interfaces from Core layer
}
```

**Queries** (Read Data):
```csharp
// Application/Features/Products/Queries/GetProductsQuery.cs
public class GetProductsQuery : QueryBase<Result<PaginatedResult<ProductDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}
```

#### Repository Pattern
Abstracts data access logic:
```csharp
// Core/Interfaces/IRepository.cs - Interface defined in Core (uses System.Linq.Expressions, NOT EF Core)
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, ...);
    // No EF Core types - uses standard LINQ expressions
}

// Data/Repositories/Repository.cs - Implementation in Data layer
public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;
    // EF Core implementation details hidden from Core and Application

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }
}

// Usage in Application layer handlers
var repository = _unitOfWork.Repository<Product>();
var product = await repository.GetByIdAsync(id, cancellationToken);
```

#### Unit of Work Pattern
Coordinates multiple repositories and manages transactions:
```csharp
// Core/Interfaces/IUnitOfWork.cs
public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

// Usage with TransactionBehavior
await _unitOfWork.BeginTransactionAsync();
try
{
    // Multiple database operations
    await _unitOfWork.SaveChangesAsync();
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```k

### 3. Asynchronous Programming

#### Async/Await Best Practices
```csharp
public async Task<Result<Guid>> Handle(
    CreateProductCommand request,
    CancellationToken cancellationToken)
{
    // Proper async await with cancellation token support
    await repository.AddAsync(product, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
}
```

#### Parallel Execution for Performance
```csharp
var productsTask = _unitOfWork.ExecuteQueryAsync<ProductDto>(sql, parameters);
var countTask = _unitOfWork.ExecuteQueryAsync<int>(countSql, parameters);
await Task.WhenAll(productsTask, countTask);
```

### 4. Security Implementation

#### JWT Authentication & Authorization
```csharp
[Authorize] // Requires authentication
[Authorize(Roles = "Admin")] // Requires specific role

// Controller uses interface, not concrete class
public class AuthController(
    IUnitOfWork unitOfWork,
    IJwtTokenService jwtTokenService, // Interface defined in Core, implemented in Infrastructure
    ICryptographyService cryptographyService,
    ILogger<AuthController> logger) : ControllerBase
```

JWT configuration in `Program.cs`:
```csharp
.AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});
```

#### Secret Management with Azure KeyVault
```csharp
// Core/Interfaces/ISecretManager.cs
public interface ISecretManager
{
    Task<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default);
}

// Infrastructure/Services/AzureKeyVaultSecretManager.cs
public class AzureKeyVaultSecretManager : ISecretManager
{
    private readonly SecretClient _secretClient;
    // Azure SDK implementation hidden from Core and Application layers
    
    public async Task<string?> GetSecretAsync(string key)
    {
        var secret = await _secretClient.GetSecretAsync(key);
        return secret.Value.Value;
    }
}
```

#### Encryption & Hashing
```csharp
// Infrastructure/Services/CryptographyService.cs implements ICryptographyService
// AES-256 Encryption
var encrypted = _cryptographyService.Encrypt(plainText);
var decrypted = _cryptographyService.Decrypt(encrypted);

// Password Hashing with PBKDF2
var hash = _cryptographyService.HashPassword(password);
var isValid = _cryptographyService.VerifyPassword(password, hash);

// SHA-256 Hashing
var hash = _cryptographyService.ComputeSha256Hash(input);
```

### 5. Database Patterns with SQL Server

#### Entity Framework Core Configuration

**DbContext Setup**:
```csharp
// Data/Persistence/ApplicationDbContext.cs
public class ApplicationDbContext : DbContext
{
    // Automatic audit fields on save
    public override async Task<int> SaveChangesAsync()
    {
        // Set CreatedAt, UpdatedAt, etc.
    }
}
```

**Entity Configuration with Fluent API**:
```csharp
// Data/Persistence/Configurations/ProductConfiguration.cs
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Price)
            .HasColumnType("decimal(18,2)");
        
        builder.HasIndex(p => p.Sku).IsUnique();
    }
}
```

#### Performance Optimizations

**NoTracking for Read-Only Queries**:
```csharp
return await _dbSet.AsNoTracking().ToListAsync();
```

**Dapper for Complex Queries**:
```csharp
public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(string sql, object parameters)
{
    var connection = _context.Database.GetDbConnection();
    return await connection.QueryAsync<T>(sql, parameters);
}
```

**Query Splitting**:
```csharp
options.UseSqlServer(connectionString, sqlOptions => {
    sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
});
```

#### Database Features

**Indexes for Performance**:
```csharp
modelBuilder.Entity<Product>()
    .HasIndex(p => new { p.CategoryId, p.IsActive });
```

**Soft Delete with Global Query Filters**:
```csharp
modelBuilder.Entity<Product>()
    .HasQueryFilter(p => !p.IsDeleted);
```

**Optimistic Concurrency**:
```csharp
builder.Property(p => p.RowVersion).IsRowVersion();
```

**Audit Trail**:
```csharp
// Core/Entities/AuditLog.cs
public class AuditLog : BaseEntity
{
    public string EntityName { get; set; }
    public string Action { get; set; }
    public string OldValues { get; set; }
    public string NewValues { get; set; }
}
```

#### Transaction Management with TransactionBehavior
```csharp
// Application/Behaviors/TransactionBehavior.cs
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;
    
    // Automatically wraps commands in database transactions
    // Commits on success, rolls back on exception
    // Skips transactions for queries (read-only operations)
}
```
#### Database Seeding with Interface Pattern

**Runtime seeding with random GUIDs per environment:**

```csharp
// Data/Seeders/ISeeder.cs public interface ISeeder<in TContext> where TContext : DbContext { Task SeedAsync(TContext context, CancellationToken cancellationToken = default); }
// Example: RoleSeeder public class RoleSeeder : ISeeder<ApplicationDbContext> { public async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken) { if (await context.Roles.AnyAsync(cancellationToken)) return;
    var roles = new List<Role>
    {
        new() { Id = Guid.NewGuid(), Name = "Admin", ... },
        new() { Id = Guid.NewGuid(), Name = "User", ... }
    };
    
    await context.Roles.AddRangeAsync(roles, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);
}
}
```

**Adding a new seeder:** Create class implementing `ISeeder<ApplicationDbContext>` → Register: `services.AddScoped<ISeeder<ApplicationDbContext>, MySeeder>();`

✅ No seed data in migrations • ✅ Random GUIDs • ✅ Idempotent • ✅ Auto-discovery

### 6. System Interoperability

#### Windows Registry Access
```csharp
// Infrastructure/Services/SystemInteropService.cs implements ISystemInteropService
var value = await _systemInteropService.ReadRegistryValueAsync(
    "HKEY_LOCAL_MACHINE\\Software\\MyApp",
    "ConfigValue");
```

#### PowerShell Execution
```csharp
var result = await _systemInteropService.ExecutePowerShellScriptAsync(
    "Get-Process | Select-Object -First 5",
    parameters);
```

#### Windows Service Management
```csharp
var status = await _systemInteropService.GetServiceStatusAsync("MyService");
await _systemInteropService.StartServiceAsync("MyService");
await _systemInteropService.StopServiceAsync("MyService");
```

### 7. Validation with FluentValidation

```csharp
// Application/Features/Products/Commands/CreateProductCommandValidator.cs
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
        
        RuleFor(x => x.Price)
            .GreaterThan(0)
            .LessThan(1000000);
    }
}
```

### 8. MediatR Pipeline Behaviors ✅

**Validation Behavior**:
```csharp
// Application/Behaviors/ValidationBehavior.cs
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    
    // Automatically validates all requests before handling
    // Throws ValidationException if validation fails
}
```

**Performance Monitoring Behavior**:
```csharp
// Application/Behaviors/PerformanceBehavior.cs
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    
    // Logs warning for requests taking longer than 500ms
    // Uses structured logging with Serilog
}
```

**Transaction Management Behavior**:
```csharp
// Application/Behaviors/TransactionBehavior.cs
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;
    
    // Automatically wraps commands in database transactions
    // Commits on success, rolls back on exception
    // Skips transactions for queries (read-only operations)
}
```

**Pipeline Order** (configured in `Program.cs`):
1. **PerformanceBehavior** - Measures execution time
2. **ValidationBehavior** - Validates request
3. **TransactionBehavior** - Manages database transaction

## ➡️ Code Quality & Formatting

This template includes comprehensive code quality configuration enforcing consistent style and catching common issues:

### Configuration Files

- **`.editorconfig`** - Code style rules, formatting preferences, naming conventions
- **`Directory.Build.props`** - MSBuild properties for code analysis
- **`.vscode/settings.json`** - VS Code format-on-save configuration
- **`.vscode/extensions.json`** - Recommended VS Code extensions

### Key Features

✅ **Unused Imports Warning** - `IDE0005` warns about unnecessary using directives
✅ **Code Style Enforcement** - Consistent formatting across all files
✅ **Naming Conventions** - Interfaces start with `I`, private fields with `_`, async methods end with `Async`
✅ **Code Analyzers** - Full .NET analyzers enabled with strict rules
✅ **Format on Save** - Automatic formatting in VS Code and Rider
✅ **Build-Time Validation** - Code style violations cause build warnings

### Quick Commands

```bash
# Check code formatting
dotnet format --verify-no-changes

# Fix code formatting
dotnet format

# Build with code analysis
dotnet build

# Remove unused imports (Visual Studio)
Ctrl+R, Ctrl+G
```

### IDE Setup

#### Visual Studio 2022
- Format Document: `Ctrl+K, Ctrl+D`
- Format Selection: `Ctrl+K, Ctrl+F`
- Remove Unused Usings: `Ctrl+R, Ctrl+G`
- EditorConfig automatically applied

#### Visual Studio Code
- Format Document: `Shift+Alt+F`
- Format on Save: Already configured in `.vscode/settings.json`
- Install recommended extensions when prompted

#### JetBrains Rider
- Format Document: `Ctrl+Alt+Enter`
- Optimize Imports: `Ctrl+Alt+O`
- EditorConfig automatically applied

### Code Quality Rules

- ✅ File-scoped namespaces (C# 10+)
- ✅ Always use braces for control statements
- ✅ Private fields must start with underscore
- ✅ Interfaces must start with 'I'
- ✅ Async methods must end with 'Async'
- ✅ Remove unused code warnings
- ✅ Unnecessary cast warnings
- ✅ Unreachable code warnings

### Documentation

See **[docs/CODE_QUALITY.md](docs/CODE_QUALITY.md)** for complete details on:
- All code quality rules and their severity
- IDE configuration for Visual Studio, VS Code, and Rider
- CI/CD integration examples
- Troubleshooting common issues
- Best practices for team collaboration

## ➡️ Getting Started

### Prerequisites
- .NET 10 SDK
- SQL Server or SQL Server LocalDB
- Visual Studio 2022 or VS Code
- (Optional) Azure subscription for KeyVault

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/paz-dev-com/CleanApiTemplate.git
   cd CleanApiTemplate
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Format check (optional)**
   ```bash
   dotnet format --verify-no-changes
   ```

4. **Update connection string** in `appsettings.json`

5. **Run database migrations**:
   ```bash
   dotnet ef database update --project CleanApiTemplate.Data --startup-project CleanApiTemplate.API
   ```

6. **Configure secrets**:
   ```bash
   cd CleanApiTemplate.API
   dotnet user-secrets set "JwtSettings:SecretKey" "YourSecretKeyHere-MinimumLength32Characters!"
   ```

7. **Run the application**:
   ```bash
   dotnet run --project CleanApiTemplate.API
   ```

8. **Access Swagger UI**: `https://localhost:5001/swagger`

## ➡️ Testing

Unit testing is easy thanks to interface-based design:

```csharp
[Fact]
public async Task Handle_ValidProduct_ReturnsSuccessResult()
{
    // Arrange
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    var mockJwtService = new Mock<IJwtTokenService>(); // Mockable!
    var mockCrypto = new Mock<ICryptographyService>(); // Mockable!
    var mockCurrentUser = new Mock<ICurrentUserService>();
    var mockLogger = new Mock<ILogger<CreateProductCommandHandler>>();
    
    var handler = new CreateProductCommandHandler(
        mockUnitOfWork.Object,
        mockCurrentUser.Object,
        mockLogger.Object);
    
    // Act & Assert...
}
```

All behaviors are also fully testable:
```csharp
[Fact]
public async Task PerformanceBehavior_LogsWarning_WhenRequestTakesLongerThan500ms()
{
    // Arrange
    var mockLogger = new Mock<ILogger<PerformanceBehavior<TestRequest, TestResponse>>>();
    var behavior = new PerformanceBehavior<TestRequest, TestResponse>(mockLogger.Object);
    
    // Act - simulate slow request
    
    // Assert - verify warning was logged
    mockLogger.Verify(x => x.Log(
        LogLevel.Warning,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Long Running Request")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
}
```

## ➡️ Documentation

### Comprehensive Guides

- **[SETUP.md](CleanApiTemplate.API/docs/SETUP.md)** - Complete setup and installation guide
- **[CODE_QUALITY.md](docs/CODE_QUALITY.md)** - Code formatting and quality rules ✨ NEW
- **[SECURITY.md](CleanApiTemplate.API/docs/SECURITY.md)** - Security best practices
- **[DATABASE.md](CleanApiTemplate.API/docs/DATABASE.md)** - Database design and optimization
- **[TESTING.md](TESTING.md)** - Complete test suite documentation

## ➡️ Best Practices Enforced

### Code Quality ✨ NEW
- ✅ Consistent code formatting via EditorConfig
- ✅ Unused imports automatically detected
- ✅ Naming conventions enforced
- ✅ Code analyzers enabled
- ✅ Build-time style validation

## ➡️ Clean Architecture Compliance Checklist

- ✅ **Core layer (Domain) has ZERO dependencies** - Pure C# only
- ✅ **Application layer has zero infrastructure dependencies** - Only MediatR and FluentValidation
- ✅ **All external dependencies abstracted via interfaces in Core**
- ✅ **Dependencies flow inward** (API/Infrastructure/Data → Application → Core, never outward)
- ✅ **Domain entities are pure C# classes in Core**
- ✅ **Business logic is in Application layer, not in API, Infrastructure, or Data**
- ✅ **Infrastructure services (JWT, Crypto, KeyVault, SystemInterop) in Infrastructure layer**
- ✅ **Data access (EF Core, Repositories) in Data layer**
- ✅ **Presentation concerns (HttpContext, Controllers) in API layer**
- ✅ **100% testable through mocking all interfaces**
- ✅ **CQRS pattern properly implemented in Application layer**
- ✅ **Pipeline behaviors for cross-cutting concerns in Application layer**

## ➡️ Additional Resources

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Microsoft .NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [EF Core Best Practices](https://docs.microsoft.com/en-us/ef/core/performance/)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
