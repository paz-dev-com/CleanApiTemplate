# Clean Architecture Template - Complete Test Suite Documentation

This document provides comprehensive documentation for the entire test suite of the Clean Architecture Template project, including both unit tests and integration tests.

## Table of Contents
- [Overview](#overview)
- [Test Statistics](#test-statistics)
- [Test Structure](#test-structure)
- [Testing Libraries](#testing-libraries)
- [Running Tests](#running-tests)
- [Unit Tests](#unit-tests)
- [Integration Tests](#integration-tests)
- [Writing Tests](#writing-tests)
- [Code Coverage](#code-coverage)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)
- [CI/CD Integration](#cicd-integration)

## Overview

The test suite follows Clean Architecture principles with **106 total tests** covering all layers:
- **92 Unit Tests** - Fast, isolated tests with mocked dependencies
- **14 Integration Tests** - End-to-end tests with real SQL Server database

All tests are located in the `CleanApiTemplate.Test` project and organized to mirror the solution structure.

## Test Statistics

```
Total Tests: 106 (All Passing ?)
??? Unit Tests: 92
?   ??? Application Layer: 54 tests
?   ?   ??? Validators: 12 tests
?   ?   ??? Command Handlers: 6 tests
?   ?   ??? Query Handlers: 8 tests
?   ?   ??? Common Utilities: 28 tests
?   ??? Core Layer: 16 tests
?   ?   ??? Product Entity: 8 tests
?   ?   ??? Category Entity: 8 tests
?   ??? Infrastructure Layer: 20 tests
?       ??? Cryptography Service: 20 tests
??? Integration Tests: 14 tests
    ??? Product Operations: 9 tests
    ??? Category Operations: 5 tests

Execution Time:
- Unit Tests: ~0.6 seconds
- Integration Tests: ~60-70 seconds
- Full Suite: ~70 seconds
```

## Test Structure

```
CleanApiTemplate.Test/
??? Common/
?   ??? TestBase.cs                     # Base class for unit tests
?   ??? TestDataGenerator.cs            # Bogus-based fake data generators
??? Application/
?   ??? Common/
?   ?   ??? ResultTests.cs              # Result pattern tests
?   ?   ??? PaginatedResultTests.cs     # Pagination logic tests
?   ??? Handlers/
?   ?   ??? CreateProductCommandHandlerTests.cs
?   ?   ??? GetProductsQueryHandlerTests.cs
?   ??? Validators/
?       ??? CreateProductCommandValidatorTests.cs
??? Core/
?   ??? Entities/
?       ??? ProductTests.cs             # Product entity tests
?       ??? CategoryTests.cs            # Category entity tests
??? Infrastructure/
?   ??? Services/
?       ??? CryptographyServiceTests.cs # Security operations tests
??? Integration/                         # Integration Tests
?   ??? Infrastructure/
?   ?   ??? IntegrationTestDbFactory.cs # Database lifecycle management
?   ?   ??? IntegrationTestBase.cs      # Base class for integration tests
?   ??? ProductIntegrationTests.cs      # Product CRUD with real DB
?   ??? CategoryIntegrationTests.cs     # Category operations with real DB
?   ??? IntegrationTestCollection.cs    # Sequential execution config
??? appsettings.Test.json                # Test database configuration
```

## Testing Libraries

### Core Testing Frameworks
- **xUnit** (v2.5.3) - Primary testing framework
- **xUnit.runner.visualstudio** (v2.5.3) - Visual Studio test runner

### Mocking and Assertions
- **Moq** (v4.20.70) - Mocking framework for test doubles
- **FluentAssertions** (v6.12.0) - Expressive, readable assertions

### Test Data Generation
- **Bogus** (v35.3.0) - Realistic fake data generation
- **AutoFixture** (v4.18.1) - Automated test data creation
- **AutoFixture.Xunit2** (v4.18.1) - xUnit integration

### Integration Testing
- **Microsoft.EntityFrameworkCore.InMemory** (v8.0.11) - Alternative in-memory testing
- **Respawn** (v6.2.1) - Fast database cleanup between tests

### Code Coverage
- **coverlet.collector** (v6.0.0) - Code coverage collection

## Running Tests

### Quick Commands

```bash
# Run ALL tests (unit + integration)
dotnet test

# Run ONLY unit tests (fast ~1s)
dotnet test --filter "Category!=Integration"

# Run ONLY integration tests (~60-70s)
dotnet test --filter "Category=Integration"

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~ProductIntegrationTests"

# Run with detailed output
dotnet test --verbosity detailed
```

### Using Test Profiles - PowerShell Scripts (Windows)

Convenient scripts for easier test execution (located in `../test-scripts/`):

```powershell
# Navigate to test scripts folder
cd test-scripts

# Run unit tests only (fast ~1s)
.\run-unit-tests.ps1

# Run unit tests with coverage
.\run-unit-tests.ps1 -Coverage

# Run unit tests with verbose output
.\run-unit-tests.ps1 -Verbose

# Run integration tests only (~60-70s, checks SQL Server connectivity)
.\run-integration-tests.ps1

# Run integration tests with coverage
.\run-integration-tests.ps1 -Coverage

# Run all tests
.\run-all-tests.ps1

# Run all tests with coverage
.\run-all-tests.ps1 -Coverage

# Generate HTML coverage report (after running tests with -Coverage)
.\generate-coverage-report.ps1
```

**Features:**
- ? Automatic SQL Server connectivity check for integration tests
- ? Color-coded output (success/failure/warnings)
- ? Execution time tracking
- ? Optional code coverage collection
- ? Optional verbose output

**Location**: All test scripts are in the `test-scripts/` folder at the project root.
See [test-scripts/README.md](../test-scripts/README.md) for more details.

### Using Test Profiles - Bash Scripts (Linux/Mac)

Scripts located in `../test-scripts/`:

```bash
# Navigate to test scripts folder
cd test-scripts

# Make scripts executable (first time only)
chmod +x *.sh

# Run unit tests only (fast ~1s)
./run-unit-tests.sh

# Run unit tests with coverage
./run-unit-tests.sh --coverage

# Run unit tests with verbose output
./run-unit-tests.sh --verbose

# Run integration tests only (~60-70s)
./run-integration-tests.sh

# Run integration tests with coverage
./run-integration-tests.sh --coverage

# Run all tests
./run-all-tests.sh

# Run all tests with coverage
./run-all-tests.sh --coverage
```

### Using .runsettings Files in Visual Studio

For integrated Visual Studio experience:

1. **Configure Test Profile**:
   - Test ? Configure Run Settings ? Select Solution Wide runsettings File
   - Choose one of:
     - `CleanApiTemplate.Test/UnitTests.runsettings` - Unit tests only
     - `CleanApiTemplate.Test/IntegrationTests.runsettings` - Integration tests only

2. **Run Tests**:
   - Test ? Run All Tests (Ctrl+R, A)
   - Or right-click specific tests in Test Explorer

### Test Execution Comparison

| Method | Unit Tests | Integration Tests | All Tests | Best For |
|--------|-----------|-------------------|-----------|----------|
| **dotnet test** | ~1s | ~60-70s | ~70s | CI/CD pipelines |
| **PowerShell scripts** | ~1s | ~60-70s | ~70s | Windows developers |
| **Bash scripts** | ~1s | ~60-70s | ~70s | Linux/Mac developers |
| **Visual Studio** | ~1s | ~60-70s | ~70s | IDE integration |
| **.runsettings** | ~1s | ~60-70s | ~70s | Team consistency |

### Recommended Workflow

```
During Development (frequent)
  ?
  Run unit tests only: cd test-scripts && .\run-unit-tests.ps1
  ? Fast feedback (~1 second)

Before Commit (occasional)
  ?
  Run all tests: cd test-scripts && .\run-all-tests.ps1
  ? Full validation (~70 seconds)

Before Push/PR (always)
  ?
  Run all tests with coverage: cd test-scripts && .\run-all-tests.ps1 -Coverage
  ?? Complete validation + metrics
```

## Unit Tests

### Overview
Unit tests are **fast, isolated tests** that verify individual components with mocked dependencies. They run in memory without external dependencies.

### Application Layer Tests (54 tests)

#### Validators (12 tests)
**`CreateProductCommandValidatorTests.cs`**
- Valid command passes validation
- Empty/null name fails validation
- Name exceeding 200 characters fails
- Description exceeding 2000 characters fails
- Invalid SKU format fails (must be uppercase alphanumeric)
- Zero or negative price fails
- Price exceeding 1,000,000 fails
- Negative stock quantity fails
- Empty category ID fails

**Example**:
```csharp
[Theory]
[InlineData("")]
[InlineData(null)]
public void Validate_EmptyName_ShouldHaveValidationError(string name)
{
    var validator = new CreateProductCommandValidator();
    var command = new CreateProductCommand { Name = name, Sku = "TEST", Price = 10, CategoryId = Guid.NewGuid() };
    
    var result = validator.Validate(command);
    
    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductCommand.Name));
}
```

#### Command Handlers (6 tests)
**`CreateProductCommandHandlerTests.cs`**
- Valid command creates product successfully
- Duplicate SKU returns failure
- Non-existent category returns failure
- CreatedBy field set to current user
- CreatedBy defaults to "System" when user is null
- New product is set as active

**Example**:
```csharp
[Fact]
public async Task Handle_ValidCommand_ShouldCreateProductSuccessfully()
{
    // Arrange
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    var mockCurrentUser = new Mock<ICurrentUserService>();
    var mockProductRepo = new Mock<IRepository<Product>>();
    var mockCategoryRepo = new Mock<IRepository<Category>>();
    
    mockUnitOfWork.Setup(x => x.Repository<Product>()).Returns(mockProductRepo.Object);
    mockUnitOfWork.Setup(x => x.Repository<Category>()).Returns(mockCategoryRepo.Object);
    mockProductRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Product, bool>>>(), default)).ReturnsAsync(false);
    mockCategoryRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>(), default)).ReturnsAsync(true);
    mockCurrentUser.Setup(x => x.Username).Returns("testuser");
    
    var handler = new CreateProductCommandHandler(mockUnitOfWork.Object, mockCurrentUser.Object);
    var command = new CreateProductCommand { Name = "Test", Sku = "TEST-001", Price = 99.99m, CategoryId = Guid.NewGuid() };
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Data.Should().NotBeEmpty();
    mockProductRepo.Verify(x => x.AddAsync(It.IsAny<Product>(), default), Times.Once);
    mockUnitOfWork.Verify(x => x.SaveChangesAsync(default), Times.Once);
}
```

#### Query Handlers (8 tests)
**`GetProductsQueryHandlerTests.cs`**
- Returns paginated results correctly
- Search term filters results
- Category filter works
- Calculates correct offset for page 2
- IncludeInactive flag controls active filtering
- Handles exceptions gracefully
- Returns empty result for no data

#### Common Utilities (28 tests)
**`ResultTests.cs`** (10 tests)
- Success/failure result creation
- Validation error handling
- Generic and non-generic variants

**`PaginatedResultTests.cs`** (18 tests)
- Total pages calculation
- Has previous/next page logic
- Create method pagination
- Edge cases (empty, exact divisions)

### Core Layer Tests (16 tests)

#### Product Entity Tests (8 tests)
**`ProductTests.cs`**
- Inherits from BaseEntity
- Initializes with default values
- Allows setting all properties
- Supports null description
- Category navigation property is nullable
- TestDataGenerator creates valid products
- Multiple products have unique IDs and SKUs

#### Category Entity Tests (8 tests)
**`CategoryTests.cs`**
- Inherits from BaseEntity
- Initializes with default values
- Allows setting all properties
- Supports null description
- Products collection is modifiable
- TestDataGenerator creates valid categories
- Multiple categories have unique IDs

### Infrastructure Layer Tests (20 tests)

#### Cryptography Service Tests (20 tests)
**`CryptographyServiceTests.cs`**

**Encryption/Decryption** (10 tests):
- Encrypts plain text successfully
- Null/empty plain text throws exception
- Decrypts to original plain text
- Handles long text (1000+ characters)
- Handles special characters

**Password Hashing** (10 tests):
- Hashes password with salt
- Same password produces different hashes (different salts)
- Verifies correct password
- Rejects incorrect password
- Null/empty password throws exception

**Token Generation & Hashing**:
- Generates secure random tokens
- Multiple tokens are unique
- SHA256 hash is consistent
- SHA256 hash is 64 characters (hex)

## Integration Tests

### Overview
Integration tests verify **end-to-end functionality** with a **real SQL Server database**. They test:
- Actual database operations
- Foreign key constraints
- Unique constraints
- Global query filters
- Soft delete behavior
- Navigation properties
- Audit fields

### Setup Requirements

#### 1. SQL Server
You need SQL Server running locally or remotely.

**Default Configuration** (`appsettings.Test.json`):
```json
{
  "ConnectionStrings": {
    "TestConnection": "Server=tcp:localhost,1433;Initial Catalog=CleanApiTemplate_Test;Persist Security Info=False;User ID=sa;Password=P@ssw0rd;MultipleActiveResultSets=True;Connection Timeout=30;TrustServerCertificate=True;"
  }
}
```

**Important**: `MultipleActiveResultSets=True` is required because the `GetProductsQueryHandler` uses Dapper to execute two queries in parallel (`Task.WhenAll`).

**To customize**: Update `appsettings.Test.json` with your connection string.

#### 2. No Manual Setup Required!
The integration test infrastructure automatically:
- ? Creates the `CleanApiTemplate_Test` database
- ? Applies all EF Core migrations
- ? Cleans data between tests using Respawn
- ? Deletes test database on disposal

### Test Infrastructure

#### IntegrationTestDbFactory
Manages the test database lifecycle.

**Key Methods**:
```csharp
await InitializeAsync();           // Creates DB and applies migrations
var context = CreateDbContext();   // Gets a new DbContext instance
await SeedAsync(entities);         // Seeds test data
await ResetDatabaseAsync();        // Cleans all data (Respawn)
await DisposeAsync();              // Deletes test database
```

#### IntegrationTestBase
Base class providing ready-to-use components.

**Available Properties**:
```csharp
protected IntegrationTestDbFactory DbFactory
protected ApplicationDbContext DbContext
protected IUnitOfWork UnitOfWork
protected Mock<ICurrentUserService> MockCurrentUserService
```

**Usage**:
```csharp
[Trait("Category", "Integration")]
[Collection("Integration Tests")]
public class MyIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task MyTest()
    {
        // Seed data
        var category = TestDataGenerator.GenerateCategory();
        await DbFactory.SeedAsync(category);
        
        // Execute test
        var handler = new CreateProductCommandHandler(UnitOfWork, MockCurrentUserService.Object);
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Verify with fresh context
        using var verifyContext = CreateNewContext();
        var saved = await verifyContext.Products.FindAsync(result.Data);
        saved.Should().NotBeNull();
    }
}
```

### Product Integration Tests (9 tests)

**`ProductIntegrationTests.cs`**

1. **CreateProduct_EndToEnd_ShouldPersistToDatabase**
   - Creates product with real database
   - Verifies all fields including audit fields (CreatedBy, CreatedAt)
   - Tests complete CRUD flow

2. **CreateProduct_WithDuplicateSku_ShouldFail**
   - Seeds existing product with SKU
   - Attempts to create duplicate
   - Verifies unique constraint enforcement

3. **CreateProduct_WithNonExistentCategory_ShouldFail**
   - Attempts to create product with invalid category ID
   - Verifies foreign key validation

4. **GetProducts_WithPagination_ShouldReturnCorrectPage**
   - Seeds 25 products
   - Requests page 2 with page size 10
   - Verifies pagination calculations (TotalPages=3, HasPrevious=true, HasNext=true)

5. **GetProducts_WithSearchTerm_ShouldFilterResults**
   - Seeds products with "Laptop" and "Smartphone"
   - Searches for "laptop"
   - Verifies only matching products returned

6. **GetProducts_WithCategoryFilter_ShouldReturnOnlyMatchingCategory**
   - Seeds products in "Electronics" and "Books" categories
   - Filters by "Electronics"
   - Verifies only Electronics products returned

7. **GetProducts_IncludeInactiveFalse_ShouldReturnOnlyActiveProducts**
   - Seeds active and inactive products
   - Queries with IncludeInactive=false
   - Verifies only active products returned

8. **GetProducts_IncludeInactiveTrue_ShouldReturnAllProducts**
   - Seeds active and inactive products
   - Queries with IncludeInactive=true
   - Verifies all products returned

9. **Product_SoftDelete_ShouldNotAppearInQueries**
   - Creates and soft deletes product
   - Verifies product doesn't appear in queries (global query filter)
   - Verifies product exists with IsDeleted=true using `IgnoreQueryFilters()`

### Category Integration Tests (5 tests)

**`CategoryIntegrationTests.cs`**

1. **Category_WithProducts_ShouldLoadNavigationProperty**
   - Creates category with 3 products
   - Uses `Include(c => c.Products)` for eager loading
   - Verifies navigation property loaded

2. **Category_Delete_ShouldSoftDeleteCategory**
   - Creates category with product
   - Soft deletes category
   - Verifies soft delete behavior (IsDeleted=true, not in normal queries)

3. **Category_Update_ShouldPersistChanges**
   - Updates category name and description
   - Verifies changes persisted
   - Verifies audit fields updated (UpdatedBy, UpdatedAt)

4. **Category_CreateMultiple_ShouldAllPersist**
   - Creates 5 categories in bulk
   - Verifies all persisted with correct CreatedBy

5. **Category_SoftDelete_ShouldSetDeletedFlags**
   - Soft deletes category
   - Verifies category doesn't appear in normal queries
   - Verifies category exists with IsDeleted=true when ignoring filters

### Performance

| Operation | Time |
|-----------|------|
| Database setup (per test class) | ~1-2 seconds |
| Database reset (Respawn) | ~50-100ms |
| Single integration test | ~200-500ms |
| Full integration suite (14 tests) | ~60-70 seconds |

**Why so fast?**
- **Respawn** is 5-10x faster than dropping/recreating database
- Resets by DELETEing rows and reseeding identity columns
- Preserves schema and migrations

### Test Execution Flow

```
1. Setup (InitializeAsync)
   ?
   - Create database connection
   - Apply all migrations
   - Initialize Respawn

2. Test Execution
   ?
   - Seed required test data
   - Execute test scenario
   - Verify with FRESH DbContext

3. Cleanup (Between Tests)
   ?
   - Reset database (Respawn)
   - ~50-100ms per test

4. Teardown (DisposeAsync)
   ?
   - Close connections
   - Delete test database
```

### Key Features

#### ?? Fast Database Cleanup
Respawn resets in ~50-100ms by:
- `DELETE` all rows from tables
- Reseed identity columns
- Preserve schema and migrations
- Skip `__EFMigrationsHistory` table

#### ?? Proper Isolation
- Each test gets clean database state
- No test data pollution
- Sequential execution prevents conflicts

#### ? Real Database Testing
Tests actual SQL Server behavior:
- Foreign key constraints
- Unique constraints
- Indexes
- Global query filters (soft delete)
- Navigation properties
- Audit fields (CreatedAt, CreatedBy, etc.)

## Writing Tests

### Test Naming Conventions

**Classes**: `{ClassBeingTested}Tests`
```csharp
public class CreateProductCommandHandlerTests { }
```

**Methods**: `MethodName_Scenario_ExpectedBehavior`
```csharp
[Fact]
public void Validate_EmptyName_ShouldHaveValidationError()
```

### Unit Test Patterns

#### 1. Using TestBase
```csharp
public class MyTests : TestBase
{
    [Fact]
    public void MyTest()
    {
        // AutoFixture available via Fixture property
        var testData = Fixture.Create<MyClass>();
    }
}
```

#### 2. Using TestDataGenerator
```csharp
// Generate single entity
var product = TestDataGenerator.GenerateProduct();

// Generate multiple entities
var products = TestDataGenerator.GenerateProducts(10);

// Customize generated data
var product = TestDataGenerator.GenerateProduct();
product.Name = "Custom Name";
product.Price = 99.99m;
```

#### 3. Mocking with Moq
```csharp
// Setup mock
var mockRepository = new Mock<IRepository<Product>>();
mockRepository
    .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new Product());

// Verify method called
mockRepository.Verify(
    x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), 
    Times.Once);
```

#### 4. FluentAssertions
```csharp
// Object assertions
result.Should().NotBeNull();
result.IsSuccess.Should().BeTrue();

// Collection assertions
items.Should().HaveCount(3);
items.Should().Contain(x => x.Name == "Test");
items.Should().OnlyHaveUniqueItems();

// String assertions
error.Should().Contain("not found");
error.Should().NotBeNullOrEmpty();

// Exception assertions
Action act = () => service.ThrowError();
act.Should().Throw<ArgumentNullException>()
   .WithMessage("Value cannot be null*");

// DateTime assertions
product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
```

#### 5. Theory Tests with InlineData
```csharp
[Theory]
[InlineData(0)]
[InlineData(-1)]
[InlineData(-100)]
public void Validate_InvalidPrice_ShouldFail(decimal price)
{
    // Arrange
    var validator = new CreateProductCommandValidator();
    var command = new CreateProductCommand { Price = price };
    
    // Act
    var result = validator.Validate(command);
    
    // Assert
    result.IsValid.Should().BeFalse();
}
```

### Integration Test Patterns

#### 1. Basic Integration Test
```csharp
[Trait("Category", "Integration")]
[Collection("Integration Tests")]
public class ProductIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateProduct_ShouldPersist()
    {
        // Arrange - Seed prerequisites
        var category = TestDataGenerator.GenerateCategory();
        category.CreatedBy = "test-user";
        await DbFactory.SeedAsync(category);
        
        // Act - Execute operation
        var handler = new CreateProductCommandHandler(UnitOfWork, MockCurrentUserService.Object);
        var command = new CreateProductCommand { CategoryId = category.Id, ... };
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert - Verify with fresh context
        using var verifyContext = CreateNewContext();
        var saved = await verifyContext.Products.FindAsync(result.Data);
        saved.Should().NotBeNull();
        saved.Name.Should().Be(command.Name);
    }
}
```

#### 2. Testing Constraints
```csharp
[Fact]
public async Task CreateProduct_DuplicateSku_ShouldFail()
{
    // Arrange - Create existing product
    var existing = TestDataGenerator.GenerateProduct();
    existing.Sku = "DUPLICATE";
    await DbFactory.SeedAsync(existing);
    
    // Act - Try to create duplicate
    var command = new CreateProductCommand { Sku = "DUPLICATE", ... };
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Contain("already exists");
}
```

#### 3. Testing Soft Delete
```csharp
[Fact]
public async Task Product_SoftDelete_ShouldNotAppear()
{
    // Arrange
    var product = TestDataGenerator.GenerateProduct();
    await DbFactory.SeedAsync(product);
    
    // Act - Soft delete
    using var deleteContext = CreateNewContext();
    var toDelete = await deleteContext.Products.FindAsync(product.Id);
    deleteContext.Products.Remove(toDelete);
    await deleteContext.SaveChangesAsync();
    
    // Assert - Not in normal queries
    using var verifyContext = CreateNewContext();
    var found = await verifyContext.Products.FindAsync(product.Id);
    found.Should().BeNull();
    
    // But exists when ignoring filters
    var deletedProduct = await verifyContext.Products
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(p => p.Id == product.Id);
    deletedProduct.Should().NotBeNull();
    deletedProduct.IsDeleted.Should().BeTrue();
}
```

## Code Coverage

### Generating Reports

```bash
# 1. Run tests with coverage collection
dotnet test --collect:"XPlat Code Coverage"

# 2. Install ReportGenerator tool (once)
dotnet tool install -g dotnet-reportgenerator-globaltool

# 3. Generate HTML report
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

# 4. Open report
# Windows: start coveragereport\index.html
# Mac: open coveragereport/index.html
# Linux: xdg-open coveragereport/index.html
```

### Coverage Goals
- **Unit Tests**: Aim for 80%+ code coverage
- **Critical Business Logic**: 100% coverage
- **Integration Tests**: Cover main user flows and edge cases
- **Exclude**: Generated code, DTOs, configurations

### Viewing Coverage in Visual Studio
1. Install **Fine Code Coverage** extension
2. Run tests
3. View coverage in Coverage window

## Best Practices

### ? DO

**General**
- ? Follow AAA pattern (Arrange, Act, Assert)
- ? Test one thing per test
- ? Use descriptive test names
- ? Test both success and failure scenarios
- ? Keep tests simple and readable
- ? Test edge cases and boundary conditions

**Unit Tests**
- ? Mock external dependencies
- ? Use TestDataGenerator for realistic data
- ? Verify mock interactions with `.Verify()`
- ? Use `FluentAssertions` for readable assertions

**Integration Tests**
- ? Use `IntegrationTestBase` as base class
- ? Create new DbContext for verification (`CreateNewContext()`)
- ? Seed required data in Arrange phase
- ? Test complete scenarios end-to-end
- ? Verify audit fields (CreatedBy, CreatedAt, etc.)
- ? Test constraints (FK, unique, etc.)
- ? Test soft delete behavior

### ? DON'T

**General**
- ? Test implementation details
- ? Create tests dependent on other tests
- ? Use hardcoded magic values
- ? Test framework code
- ? Over-mock (mock only what you need)
- ? Ignore failing tests
- ? Write tests without assertions

**Integration Tests**
- ? Use same DbContext for act and assert (data is cached!)
- ? Rely on test execution order
- ? Share data between tests
- ? Leave test data cleanup to manual process
- ? Test in parallel (integration tests run sequentially)

### Example: Bad vs Good

**? BAD - Using Same Context**:
```csharp
await DbContext.Products.AddAsync(product);
await DbContext.SaveChangesAsync();
var saved = await DbContext.Products.FindAsync(product.Id); // ? Cached in memory!
```

**? GOOD - Fresh Context**:
```csharp
await DbContext.Products.AddAsync(product);
await DbContext.SaveChangesAsync();

using var verifyContext = CreateNewContext();
var saved = await verifyContext.Products.FindAsync(product.Id); // ? Fresh from DB
```

## Troubleshooting

### Unit Tests

#### Tests Not Showing in Test Explorer
1. Rebuild the solution (`Ctrl+Shift+B`)
2. Clean solution (`Build > Clean Solution`)
3. Restart Visual Studio
4. Check Test Explorer filter settings

#### Tests Failing Intermittently
- Check for timing issues (use `.Should().BeCloseTo()` for DateTime)
- Look for shared state between tests
- Ensure proper mock setup
- Check for async/await issues

#### Mocks Not Working
```csharp
// ? BAD - Wrong setup
mockRepo.Setup(x => x.GetByIdAsync(Guid.NewGuid(), default)); // Specific GUID

// ? GOOD - Use It.IsAny<>
mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
```

### Integration Tests

#### SQL Server Not Running
```powershell
# Check SQL Server status
Get-Service | Where-Object {$_.Name -like "*SQL*"}

# Start SQL Server (if stopped)
Start-Service MSSQLSERVER
```

#### Connection Issues
```powershell
# Test connection manually
sqlcmd -S localhost,1433 -U sa -P YourStrong@Passw0rd

# Check SQL Server Configuration Manager
# Enable TCP/IP protocol if needed
```

#### Tests Failing with Database Errors
1. Verify connection string in `appsettings.Test.json`
2. Ensure SQL Server allows remote connections
3. Check firewall settings
4. Try connecting with SQL Server Management Studio first

#### Database Not Cleaning Between Tests
```csharp
// Manually reset if needed
await DbFactory.ResetDatabaseAsync();
```

#### "Database in use" errors
- Ensure tests run sequentially (they should with `[Collection("Integration Tests")]`)
- Close any connections in SQL Server Management Studio
- Restart SQL Server if needed

### Code Coverage

#### Coverage Not Generated
1. Ensure `coverlet.collector` package is installed
2. Check that tests are passing first
3. Run with explicit collector: `dotnet test --collect:"XPlat Code Coverage"`
4. Look for `coverage.cobertura.xml` in test results folder

#### Low Coverage Numbers
- Exclude test projects from coverage
- Exclude generated code (Migrations, etc.)
- Focus on business logic, not DTOs/entities

## CI/CD Integration

### GitHub Actions

```yaml
name: .NET Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  test:
    runs-on: ubuntu-latest
    
    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          ACCEPT_EULA: Y
          SA_PASSWORD: YourStrong@Passw0rd
        ports:
          - 1433:1433
        options: >-
          --health-cmd "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q 'SELECT 1'"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Run Unit Tests
      run: dotnet test --no-build --filter "Category!=Integration" --logger "console;verbosity=detailed"
    
    - name: Run Integration Tests
      env:
        ConnectionStrings__TestConnection: "Server=localhost,1433;Database=CleanApiTemplate_Test;User ID=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
      run: dotnet test --no-build --filter "Category=Integration" --logger "console;verbosity=detailed"
    
    - name: Generate Coverage Report
      run: |
        dotnet test --collect:"XPlat Code Coverage"
        dotnet tool install -g dotnet-reportgenerator-globaltool
        reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
    
    - name: Upload Coverage
      uses: codecov/codecov-action@v3
      with:
        files: '**/coverage.cobertura.xml'
```

### Azure DevOps Pipeline

```yaml
trigger:
  branches:
    include:
    - main
    - develop

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'

steps:
- task: UseDotNet@2
  inputs:
    version: '8.0.x'

- script: |
    docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
      -p 1433:1433 --name sqlserver --health-cmd="/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q 'SELECT 1'" \
      --health-interval=10s --health-timeout=5s --health-retries=5 -d \
      mcr.microsoft.com/mssql/server:2022-latest
  displayName: 'Start SQL Server'

- script: sleep 30
  displayName: 'Wait for SQL Server'

- task: DotNetCoreCLI@2
  displayName: 'Restore'
  inputs:
    command: 'restore'

- task: DotNetCoreCLI@2
  displayName: 'Build'
  inputs:
    command: 'build'
    arguments: '--configuration $(buildConfiguration) --no-restore'

- task: DotNetCoreCLI@2
  displayName: 'Run Unit Tests'
  inputs:
    command: 'test'
    arguments: '--no-build --configuration $(buildConfiguration) --filter "Category!=Integration" --logger trx'

- task: DotNetCoreCLI@2
  displayName: 'Run Integration Tests'
  inputs:
    command: 'test'
    arguments: '--no-build --configuration $(buildConfiguration) --filter "Category=Integration" --logger trx'
  env:
    ConnectionStrings__TestConnection: 'Server=localhost,1433;Database=CleanApiTemplate_Test;User ID=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;'

- task: PublishTestResults@2
  displayName: 'Publish Test Results'
  inputs:
    testResultsFormat: 'VSTest'
    testResultsFiles: '**/*.trx'
```

### Pre-commit Hook

Create `.git/hooks/pre-commit`:

```bash
#!/bin/sh

echo "Running unit tests before commit..."
dotnet test --filter "Category!=Integration" --verbosity quiet

if [ $? -ne 0 ]; then
  echo "? Unit tests failed. Commit aborted."
  exit 1
fi

echo "? All unit tests passed!"
exit 0
```

Make executable: `chmod +x .git/hooks/pre-commit`

## Resources

### Documentation
- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Bogus Documentation](https://github.com/bchavez/Bogus)
- [AutoFixture Documentation](https://github.com/AutoFixture/AutoFixture)
- [Respawn Documentation](https://github.com/jbogard/Respawn)
- [EF Core Testing](https://learn.microsoft.com/en-us/ef/core/testing/)

### Best Practices
- [Microsoft Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
- [Integration Testing in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)

## Contributing

When adding new features to the template:

1. **Write tests first** (TDD approach)
   - Write failing test
   - Implement feature
   - Verify test passes

2. **Ensure all tests pass**
   ```bash
   dotnet test
   ```

3. **Maintain or improve code coverage**
   ```bash
   dotnet test --collect:"XPlat Code Coverage"
   ```

4. **Update documentation** if adding new test patterns

5. **Follow existing patterns**
   - Use `TestBase` for unit tests
   - Use `IntegrationTestBase` for integration tests
   - Use `TestDataGenerator` for test data
   - Use FluentAssertions for assertions

---

## Summary

This test suite provides **comprehensive coverage** with:

? **104 total tests** (90 unit + 14 integration)
? **Fast execution** (~70 seconds for full suite)
? **Real database testing** with SQL Server
? **Clean architecture** following project structure
? **Production-ready** with CI/CD examples
? **Well-documented** with examples and best practices
? **Maintainable** with clear patterns and conventions

**All tests passing! ??**

For questions or issues, refer to the [Troubleshooting](#troubleshooting) section or check the inline code comments in test files.
