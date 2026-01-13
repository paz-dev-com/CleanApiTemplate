# Setup and Installation Guide

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) or SQL Server LocalDB
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- (Optional) [Azure subscription](https://azure.microsoft.com/) for KeyVault

## Project Structure

This template implements **Clean Architecture** with **5 layers**:

```
CleanApiTemplate/
??? CleanApiTemplate.API/                # Presentation Layer (Web API)
?   ??? Controllers/                     # HTTP endpoints
?   ??? Middleware/                      # Exception handling, logging
?   ??? Services/                        # Presentation services (CurrentUserService)
?   ??? Program.cs                       # Application entry point & DI setup
?
??? CleanApiTemplate.Core/               # Domain Layer (ZERO dependencies)
?   ??? Entities/                        # Domain entities (Product, User, Category, etc.)
?   ??? Interfaces/                      # Abstractions for all external dependencies
?       ??? IRepository.cs
?       ??? IUnitOfWork.cs
?       ??? IJwtTokenService.cs
?       ??? ICryptographyService.cs
?       ??? ISecretManager.cs
?       ??? ISystemInteropService.cs
?       ??? IHealthCheckService.cs
?       ??? ICurrentUserService.cs
?
??? CleanApiTemplate.Application/        # Application Layer (Use Cases)
?   ??? Features/                        # CQRS Commands & Queries
?   ?   ??? Products/
?   ?       ??? Commands/                # CreateProductCommand, UpdateProductCommand
?   ?       ??? Queries/                 # GetProductsQuery, GetProductByIdQuery
?   ??? Behaviors/                       # MediatR pipeline behaviors
?   ?   ??? ValidationBehavior.cs        # FluentValidation integration
?   ?   ??? PerformanceBehavior.cs       # Performance monitoring
?   ?   ??? TransactionBehavior.cs       # Transaction management
?   ??? Common/                          # Shared DTOs and base classes
?   ?   ??? Result.cs
?   ?   ??? PaginatedResult.cs
?   ?   ??? CommandBase.cs
?   ?   ??? QueryBase.cs
?   ??? [Dependencies: MediatR, FluentValidation, Logging.Abstractions only]
?
??? CleanApiTemplate.Infrastructure/     # Infrastructure Layer (External Services)
?   ??? Services/                        # Infrastructure service implementations
?       ??? JwtTokenService.cs           # JWT token generation
?       ??? CryptographyService.cs       # Encryption, hashing, password management
?       ??? AzureKeyVaultSecretManager.cs # Azure KeyVault integration
?       ??? SystemInteropService.cs      # Windows Registry, PowerShell, Services
?       ??? HealthCheckService.cs        # Health check implementations
?
??? CleanApiTemplate.Data/               # Data Access Layer (Persistence)
    ??? Persistence/                     # EF Core DbContext
    ?   ??? ApplicationDbContext.cs
    ?   ??? Configurations/              # Entity configurations
    ??? Repositories/                    # Data access implementations
        ??? Repository.cs
        ??? UnitOfWork.cs
```

## Clean Architecture Principles Applied

This template strictly follows Clean Architecture with **5 distinct layers**:

- ? **Core Layer (Domain)**: Zero dependencies (no NuGet packages - pure .NET)
  - Contains domain entities and interface definitions only
  
- ? **Application Layer (Use Cases)**: Infrastructure-free
  - Only MediatR (14.0.0), FluentValidation (12.1.1), and Logging abstractions (10.0.0)
  - Contains CQRS commands/queries, handlers, and pipeline behaviors
  - No EF Core, no Azure SDK, no infrastructure dependencies
  
- ? **Infrastructure Layer (External Services)**: Infrastructure implementations
  - JWT token service, Cryptography, Azure KeyVault, System Interop, Health Checks
  - Dependencies: Azure SDK, System.Management.Automation, JWT libraries
  
- ? **Data Layer (Persistence)**: Database access
  - EF Core, Repositories, UnitOfWork, Dapper for raw SQL
  
- ? **API Layer (Presentation)**: HTTP concerns
  - Controllers, Middleware, CurrentUserService (depends on HttpContext)
  
- ? **Dependency Direction**: API/Infrastructure/Data ? Application ? Core (never outward)

## Initial Setup

### 1. Create Solution (if not exists)

```bash
dotnet new sln -n CleanApiTemplate
dotnet sln add CleanApiTemplate.API/CleanApiTemplate.API.csproj
dotnet sln add CleanApiTemplate.Core/CleanApiTemplate.Core.csproj
dotnet sln add CleanApiTemplate.Application/CleanApiTemplate.Application.csproj
dotnet sln add CleanApiTemplate.Infrastructure/CleanApiTemplate.Infrastructure.csproj
dotnet sln add CleanApiTemplate.Data/CleanApiTemplate.Data.csproj
```

### 2. Restore NuGet Packages

```bash
dotnet restore
```

If you encounter PackageSourceMapping issues, create/update `nuget.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
```

### 2.1. Verify Code Quality Configuration (Optional)

```bash
# Check code formatting
dotnet format CleanApiTemplate.sln --verify-no-changes

# If formatting issues are found, fix them
dotnet format CleanApiTemplate.sln
```

**Note:** The solution includes comprehensive code quality configuration with EditorConfig, analyzers, and formatting rules. See [CODE_QUALITY.md](../../docs/CODE_QUALITY.md) for details.

### 3. Update Configuration

Update `appsettings.json` with your connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CleanApiTemplateDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "YourSecretKeyHere-ChangeInProduction!",
    "Issuer": "CleanApiTemplate",
    "Audience": "CleanApiTemplateUsers",
    "ExpirationInMinutes": "60"
  }
}
```

### 4. Apply Database Migrations

```bash
# Install EF Core tools if not already installed
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialCreate --project CleanApiTemplate.Data --startup-project CleanApiTemplate.API

# Update database
dotnet ef database update --project CleanApiTemplate.Data --startup-project CleanApiTemplate.API
```

The application will automatically apply pending migrations on startup (configured in `Program.cs`).

### 5. Configure User Secrets (Development)

```bash
cd CleanApiTemplate.API
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:SecretKey" "YourDevelopmentSecretKeyHere123!MinimumLength32Characters"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YourConnectionString"
```

**Important**: JWT secret must be at least 32 characters for HS256 algorithm.

### 6. Run the Application

```bash
dotnet run --project CleanApiTemplate.API
```

Or press F5 in Visual Studio.

The application will:
1. Configure Serilog for structured logging
2. Register all services via Dependency Injection:
   - `AddApplicationServices()` - MediatR, FluentValidation, Behaviors
   - `AddInfrastructureServices()` - JWT, Crypto, KeyVault, SystemInterop
   - `AddDataServices()` - EF Core, Repositories, UnitOfWork
3. Apply MediatR pipeline behaviors (Performance ? Validation ? Transaction)
4. Automatically check database and apply migrations
5. Start the web server

### 7. Access Swagger UI

Navigate to: `https://localhost:5001/swagger`

Swagger UI includes JWT authentication support. Click "Authorize" and enter: `Bearer {your-jwt-token}`

## Database Migration Commands

### Create a New Migration
```bash
dotnet ef migrations add MigrationName --project CleanApiTemplate.Data --startup-project CleanApiTemplate.API
```

### Apply Migrations
```bash
dotnet ef database update --project CleanApiTemplate.Data --startup-project CleanApiTemplate.API
```

### Roll Back Migration
```bash
dotnet ef database update PreviousMigrationName --project CleanApiTemplate.Data --startup-project CleanApiTemplate.API
```

### Remove Last Migration
```bash
dotnet ef migrations remove --project CleanApiTemplate.Data --startup-project CleanApiTemplate.API
```

### Generate SQL Script
```bash
dotnet ef migrations script --project CleanApiTemplate.Data --startup-project CleanApiTemplate.API --output migration.sql
```

## Pipeline Behaviors Configuration

MediatR pipeline behaviors are executed in this order (configured in `Application/DependencyInjection.cs`):

1. **PerformanceBehavior** - Measures execution time, logs warnings for slow requests (>500ms)
2. **ValidationBehavior** - Validates requests using FluentValidation, throws exception if invalid
3. **TransactionBehavior** - Wraps commands in database transactions (commits/rollbacks automatically)

All behaviors are in the **Application layer** and use `ILogger<T>` for structured logging.

## Azure KeyVault Setup (Optional)

### 1. Create KeyVault in Azure

```bash
az keyvault create --name your-keyvault-name --resource-group your-resource-group --location eastus
```

### 2. Add Secrets

```bash
az keyvault secret set --vault-name your-keyvault-name --name "DbConnectionString" --value "your-connection-string"
az keyvault secret set --vault-name your-keyvault-name --name "JwtSecretKey" --value "your-jwt-secret"
```

### 3. Configure Access

```bash
# For your Azure identity
az keyvault set-policy --name your-keyvault-name --upn user@domain.com --secret-permissions get list

# For managed identity (in Azure)
az keyvault set-policy --name your-keyvault-name --object-id <managed-identity-object-id> --secret-permissions get list
```

### 4. Update appsettings.json

```json
{
  "AzureKeyVault": {
    "VaultUri": "https://your-keyvault-name.vault.azure.net/"
  }
}
```

The `AzureKeyVaultSecretManager` (in **Infrastructure layer**) will automatically retrieve secrets using Azure Identity.

## Development Tools

### Recommended VS Code Extensions
- C# Dev Kit
- C# (Microsoft)
- EditorConfig for VS Code
- REST Client
- SQLTools
- GitLens
- Thunder Client (API testing)

**Note:** A complete list of recommended extensions is available in `.vscode/extensions.json`. VS Code will prompt you to install them when you open the workspace.

### Recommended Visual Studio Extensions
- Productivity Power Tools
- ReSharper (optional)
- SQL Server Object Explorer
- Fine Code Coverage (for code coverage visualization)

### Code Quality Tools
- **EditorConfig**: Automatically applied in all IDEs
- **dotnet format**: Built-in code formatter (`dotnet format CleanApiTemplate.sln`)
- **Code Analyzers**: Enabled in all projects via `Directory.Build.props`

**See [CODE_QUALITY.md](../../docs/CODE_QUALITY.md) for complete code quality configuration details.**

## Testing the API

### Health Checks

#### Basic Health Check
```http
GET https://localhost:5001/health
```

Response:
```json
{
  "status": "healthy",
  "timestamp": "2024-01-01T12:00:00Z",
  "environment": "Development",
  "version": "1.0.0"
}
```

#### Database Health Check
```http
GET https://localhost:5001/health/db
```

### Sample API Calls

#### Register User (if authentication endpoint implemented)
```http
POST https://localhost:5001/api/auth/register
Content-Type: application/json

{
  "username": "testuser",
  "email": "test@example.com",
  "password": "SecurePassword123!"
}
```

#### Login
```http
POST https://localhost:5001/api/auth/login
Content-Type: application/json

{
  "username": "testuser",
  "password": "SecurePassword123!"
}
```

#### Create a Product (requires authentication)
```http
POST https://localhost:5001/api/products
Content-Type: application/json
Authorization: Bearer {your-jwt-token}

{
  "name": "Laptop",
  "description": "Gaming Laptop",
  "sku": "LAPTOP-001",
  "price": 1299.99,
  "stockQuantity": 10,
  "categoryId": "category-guid-here"
}
```

#### Get Products (with pagination)
```http
GET https://localhost:5001/api/products?pageNumber=1&pageSize=10&searchTerm=laptop
```

## Troubleshooting

### Package Restore Issues

If packages won't restore:
1. Clear NuGet cache: `dotnet nuget locals all --clear`
2. Delete `bin` and `obj` folders
3. Run `dotnet restore` again
4. Check for version conflicts in `.csproj` files

### Database Connection Issues

1. Verify SQL Server is running: `services.msc` (Windows)
2. Check connection string format
3. Ensure database exists or migrations have been applied
4. Check firewall settings
5. Enable TCP/IP in SQL Server Configuration Manager

### JWT Authentication Issues

1. Verify JWT secret key is at least 32 characters
2. Check token expiration (default: 60 minutes)
3. Ensure `Authorization: Bearer {token}` header is included
4. Validate token claims match requirements
5. Check issuer and audience match configuration

### EF Core Issues

1. Ensure connection string is correct
2. Run migrations: `dotnet ef database update`
3. Check EF Core version compatibility (this template uses 10.0.0)
4. Enable logging: Add `optionsBuilder.LogTo(Console.WriteLine)` in DbContext
5. Clear EF Core compiled models: Delete `obj` folders

### Pipeline Behavior Issues

1. Check behavior registration order in `Program.cs`
2. Verify `ILogger<T>` is registered (done automatically by ASP.NET Core)
3. Review logs in `logs/api-{date}.txt` for detailed information
4. Ensure `IUnitOfWork` is registered for TransactionBehavior

## Performance Optimization

### Development
- Use `AsNoTracking()` for read-only queries
- Enable SQL logging to identify slow queries: Check Serilog output
- Use Dapper for complex read operations (already integrated in UnitOfWork)
- Monitor performance with PerformanceBehavior (logs requests >500ms)

### Production
- Enable Response Caching
- Use Redis for distributed caching
- Implement query result caching
- Use CDN for static assets
- Enable compression
- Monitor with Application Insights or similar APM tool
- Configure connection pooling (enabled by default in EF Core)
- Use compiled queries for frequently-executed queries

## Security Checklist Before Production

- [ ] Change all default secrets and keys
- [ ] Use Azure KeyVault or AWS Secrets Manager for secrets
- [ ] Enable HTTPS only (`app.UseHttpsRedirection()`)
- [ ] Configure restrictive CORS (not `AllowAll`)
- [ ] Implement rate limiting (use AspNetCoreRateLimit package)
- [ ] Enable audit logging (AuditLog entity is included in Core/Entities)
- [ ] Use strong password policies (configure in authentication)
- [ ] Implement proper error handling (ExceptionHandlingMiddleware included)
- [ ] Keep packages up to date (use `dotnet outdated`)
- [ ] Configure database backups
- [ ] Use least privilege for database accounts
- [ ] Enable SQL Server TDE (Transparent Data Encryption)
- [ ] Configure monitoring and alerts
- [ ] Disable Swagger in production (`if (app.Environment.IsDevelopment())`)
- [ ] Remove sensitive data from logs
- [ ] Implement request/response logging (Serilog configured)

## Testing Strategy

### Unit Tests
- Test command/query handlers in **Application layer** with mocked dependencies
- Test validators independently (FluentValidation)
- Test pipeline behaviors with mocked `ILogger<T>` and `IUnitOfWork`
- Mock all interfaces from **Core layer** (IRepository, IUnitOfWork, IJwtTokenService, ICryptographyService, etc.)

**Example**:
```csharp
[Fact]
public async Task CreateProductHandler_WithValidRequest_ReturnsSuccess()
{
    // Arrange
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    var mockCurrentUser = new Mock<ICurrentUserService>();
    var mockLogger = new Mock<ILogger<CreateProductCommandHandler>>();
    
    var handler = new CreateProductCommandHandler(
        mockUnitOfWork.Object,
        mockCurrentUser.Object,
        mockLogger.Object
    );
    
    // Act & Assert...
}
```

### Integration Tests
- Test database operations in **Data layer** with test database
- Test API endpoints with WebApplicationFactory
- Verify transaction rollback scenarios with TransactionBehavior

### Performance Tests
- Benchmark critical operations
- Verify PerformanceBehavior catches slow requests (>500ms)
- Load test with tools like k6, JMeter, or Apache Bench

## Next Steps

1. ? **5-layer architecture implemented** - Core, Application, Infrastructure, Data, API
2. ? **Pure Core layer** - Zero dependencies, only domain entities and interfaces
3. ? **Infrastructure-free Application layer** - Only MediatR and FluentValidation
4. ? **Pipeline behaviors implemented** - Validation, Performance, Transaction
5. ? **Logging configured** - Structured logging with Serilog
6. Implement additional authentication endpoints (Register, Login)
7. Add more entities and features using CQRS pattern
8. Implement comprehensive unit tests (all services are mockable)
9. Add API versioning (Microsoft.AspNetCore.Mvc.Versioning)
10. Configure health checks for external dependencies
11. Set up CI/CD pipeline (GitHub Actions, Azure DevOps)
12. Add distributed caching with Redis
13. Implement SignalR for real-time features
14. Add background jobs with Hangfire or Quartz
15. Configure Application Insights for monitoring

## Logs Location

Logs are written to:
- **Console**: Visible when running the application
- **File**: `logs/api-{date}.txt` with daily rolling

Logs include:
- Application startup/shutdown
- Request/response logging (Serilog middleware)
- Performance warnings (requests >500ms via PerformanceBehavior)
- Transaction begin/commit/rollback (TransactionBehavior)
- Validation errors (ValidationBehavior)
- Exceptions with stack traces

## Support

For issues or questions:
- Check the documentation in the `/docs` folder
- Review the code comments (extensive XML documentation)
- Consult the official [.NET documentation](https://docs.microsoft.com/en-us/dotnet/)
- Review the Security and Database best practices documents
- Check the main `README.md` for architecture overview
- All interfaces are in **Core/Interfaces** for reference

## License

This template is provided as-is for educational and commercial use.
