# Database Concepts & Best Practices

## Table of Contents
1. [SQL Server Optimization](#sql-server-optimization)
2. [Entity Framework Core Patterns](#entity-framework-core-patterns)
3. [Transaction Management](#transaction-management)
4. [Security](#security)
5. [Performance Monitoring](#performance-monitoring)

## SQL Server Optimization

### Indexes

#### When to Use Indexes
- Columns frequently used in WHERE clauses
- Foreign key columns
- Columns used in JOIN operations
- Columns used in ORDER BY or GROUP BY

#### Types of Indexes
```sql
-- Clustered Index (one per table, usually on primary key)
CREATE CLUSTERED INDEX IX_Products_Id ON Products(Id);

-- Non-Clustered Index
CREATE NONCLUSTERED INDEX IX_Products_Name ON Products(Name);

-- Composite Index
CREATE INDEX IX_Products_Category_Active ON Products(CategoryId, IsActive);

-- Unique Index
CREATE UNIQUE INDEX IX_Products_Sku ON Products(Sku);

-- Filtered Index (for soft delete pattern)
CREATE INDEX IX_Products_Active ON Products(IsActive) 
WHERE IsDeleted = 0;
```

#### Index Maintenance
```sql
-- Rebuild fragmented indexes
ALTER INDEX ALL ON Products REBUILD;

-- Update statistics
UPDATE STATISTICS Products;

-- Check index fragmentation
SELECT 
    OBJECT_NAME(i.object_id) AS TableName,
    i.name AS IndexName,
    s.avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') s
INNER JOIN sys.indexes i ON s.object_id = i.object_id AND s.index_id = i.index_id;
```

### Execution Plans

#### Viewing Execution Plans
```sql
-- Enable actual execution plan in SSMS (Ctrl+M)

-- View estimated execution plan
SET SHOWPLAN_XML ON;
GO

SELECT p.*, c.Name
FROM Products p
INNER JOIN Categories c ON p.CategoryId = c.Id
WHERE p.IsActive = 1;

SET SHOWPLAN_XML OFF;
```

#### Common Issues
- **Table Scans**: Missing indexes
- **Key Lookups**: Add covering indexes
- **Nested Loops**: Check join conditions and indexes
- **Sort Operations**: Add indexes on ORDER BY columns

### Query Optimization

#### Use Appropriate Joins
```sql
-- INNER JOIN (fastest)
SELECT p.*, c.Name
FROM Products p
INNER JOIN Categories c ON p.CategoryId = c.Id;

-- LEFT JOIN (when you need all left records)
SELECT p.*, c.Name
FROM Products p
LEFT JOIN Categories c ON p.CategoryId = c.Id;

-- Avoid CROSS JOIN unless necessary
```

#### Use WHERE Instead of HAVING When Possible
```sql
-- Good
SELECT CategoryId, COUNT(*)
FROM Products
WHERE IsActive = 1
GROUP BY CategoryId;

-- Slower
SELECT CategoryId, COUNT(*)
FROM Products
GROUP BY CategoryId
HAVING MAX(IsActive) = 1;
```

#### Avoid SELECT *
```sql
-- Bad
SELECT * FROM Products;

-- Good - only select needed columns
SELECT Id, Name, Price FROM Products;
```

## Entity Framework Core Patterns

### Tracking vs NoTracking

```csharp
// Tracking (default) - for updates
var product = await context.Products.FindAsync(id);
product.Price = 99.99m;
await context.SaveChangesAsync(); // EF tracks changes

// NoTracking - for read-only queries (better performance)
var products = await context.Products
    .AsNoTracking()
    .ToListAsync();
```

### Avoiding N+1 Queries

```csharp
// BAD - N+1 Problem
var products = await context.Products.ToListAsync();
foreach (var product in products)
{
    // Each iteration makes a separate query!
    var category = await context.Categories.FindAsync(product.CategoryId);
}

// GOOD - Use Include
var products = await context.Products
    .Include(p => p.Category)
    .ToListAsync();

// BETTER - Use projection if you don't need full entity
var products = await context.Products
    .Select(p => new ProductDto
    {
        Id = p.Id,
        Name = p.Name,
        CategoryName = p.Category.Name
    })
    .ToListAsync();
```

### Projections for Performance

```csharp
// Instead of loading full entities
var products = await context.Products
    .Select(p => new
    {
        p.Id,
        p.Name,
        p.Price,
        CategoryName = p.Category.Name
    })
    .ToListAsync();
```

### Bulk Operations

```csharp
// Bad - multiple round trips
foreach (var product in products)
{
    context.Products.Add(product);
    await context.SaveChangesAsync();
}

// Good - single round trip
context.Products.AddRange(products);
await context.SaveChangesAsync();
```

### Query Splitting

```csharp
// For queries with multiple includes
var products = await context.Products
    .Include(p => p.Category)
    .Include(p => p.Reviews)
    .AsSplitQuery() // Split into multiple queries to avoid cartesian explosion
    .ToListAsync();
```

## Transaction Management

### Automatic Transaction Management with MediatR Behaviors

This template implements automatic transaction management using the **TransactionBehavior** pipeline behavior:

```csharp
// Application/Behaviors/TransactionBehavior.cs
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;
    
    public async Task<TResponse> Handle(...)
    {
        // Only apply transactions to commands (not queries)
        var isCommand = typeof(TRequest).Name.EndsWith("Command");
        
        if (!isCommand)
            return await next();
            
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            var response = await next();
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            return response;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
```

**Benefits**:
- ✅ No need to manually manage transactions in handlers
- ✅ Automatic rollback on exceptions
- ✅ Consistent transaction handling across all commands
- ✅ Queries are not wrapped in transactions (better performance)

**Usage in Command Handlers**:
```csharp
// Application/Features/Products/Commands/CreateProductCommandHandler.cs
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken ct)
    {
        // No need for explicit transaction management
        // TransactionBehavior handles it automatically
        
        var product = new Product { ... };
        await _unitOfWork.Repository<Product>().AddAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        // Transaction commits automatically on success
        // Rolls back automatically on exception
        
        return Result<Guid>.Success(product.Id);
    }
}
```

### Explicit Transactions

For complex scenarios requiring manual control:

```csharp
// Data/Repositories/UnitOfWork.cs
using var transaction = await context.Database.BeginTransactionAsync();
try
{
    // Multiple operations
    context.Products.Add(product);
    await context.SaveChangesAsync();
    
    context.Orders.Add(order);
    await context.SaveChangesAsync();
    
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### Unit of Work Pattern

The `IUnitOfWork` interface (defined in **Core/Interfaces**) provides transaction management:

```csharp
// Core/Interfaces/IUnitOfWork.cs
public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

// Implementation in Data/Repositories/UnitOfWork.cs
```

**Registered in Program.cs**:
```csharp
// Data layer DI registration
builder.Services.AddDataServices(builder.Configuration);
```

### Isolation Levels

```csharp
using var transaction = await context.Database.BeginTransactionAsync(
    System.Data.IsolationLevel.ReadCommitted);
```

#### Isolation Levels:
- **Read Uncommitted**: Dirty reads possible
- **Read Committed**: Default, prevents dirty reads
- **Repeatable Read**: Prevents non-repeatable reads
- **Serializable**: Highest isolation, prevents phantom reads
- **Snapshot**: Uses row versioning

### Deadlock Prevention

```csharp
// Always access tables in the same order
// Always keep transactions short
// Use appropriate isolation levels
// Consider using row versioning with snapshot isolation
```

## Security

### Connection Strings

```csharp
// Use integrated security when possible
"Server=localhost;Database=MyDb;Integrated Security=true;"

// Or use connection string encryption
"Server=localhost;Database=MyDb;User Id=user;Password=pass;Encrypt=true;"
```

### SQL Injection Prevention

```csharp
// NEVER do this
var sql = $"SELECT * FROM Products WHERE Name = '{userInput}'";

// Always use parameterized queries
var products = await context.Products
    .FromSqlRaw("SELECT * FROM Products WHERE Name = {0}", userInput)
    .ToListAsync();

// Or use LINQ (automatically parameterized)
var products = await context.Products
    .Where(p => p.Name == userInput)
    .ToListAsync();
```

### Database Roles and Permissions

```sql
-- Create application user with limited permissions
CREATE LOGIN AppUser WITH PASSWORD = 'SecurePassword123!';
CREATE USER AppUser FOR LOGIN AppUser;

-- Grant only necessary permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON Products TO AppUser;
GRANT SELECT, INSERT, UPDATE, DELETE ON Categories TO AppUser;

-- Don't grant DDL permissions
-- REVOKE CREATE TABLE TO AppUser;
```

### Encryption

#### Column-Level Encryption
```sql
-- Always Encrypted for sensitive data
CREATE TABLE Users
(
    Id INT PRIMARY KEY,
    Email NVARCHAR(256),
    SSN NVARCHAR(11) ENCRYPTED WITH (
        COLUMN_ENCRYPTION_KEY = MyKey,
        ENCRYPTION_TYPE = DETERMINISTIC,
        ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256'
    )
);
```

#### Transparent Data Encryption (TDE)
```sql
-- Encrypt entire database at rest
USE master;
GO
CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'SecurePassword123!';
GO
CREATE CERTIFICATE TDECert WITH SUBJECT = 'TDE Certificate';
GO
USE MyDatabase;
GO
CREATE DATABASE ENCRYPTION KEY
WITH ALGORITHM = AES_256
ENCRYPTION BY SERVER CERTIFICATE TDECert;
GO
ALTER DATABASE MyDatabase SET ENCRYPTION ON;
GO
```

## Performance Monitoring

### Identify Slow Queries

```sql
-- Top 10 slowest queries
SELECT TOP 10
    total_worker_time/execution_count AS avg_cpu_time,
    execution_count,
    SUBSTRING(text, (statement_start_offset/2)+1,
        ((CASE statement_end_offset
            WHEN -1 THEN DATALENGTH(text)
            ELSE statement_end_offset
        END - statement_start_offset)/2) + 1) AS query_text
FROM sys.dm_exec_query_stats
CROSS APPLY sys.dm_exec_sql_text(sql_handle)
ORDER BY avg_cpu_time DESC;
```

### Monitor Index Usage

```sql
-- Find unused indexes
SELECT 
    OBJECT_NAME(i.object_id) AS TableName,
    i.name AS IndexName,
    s.user_seeks,
    s.user_scans,
    s.user_lookups,
    s.user_updates
FROM sys.indexes i
LEFT JOIN sys.dm_db_index_usage_stats s 
    ON i.object_id = s.object_id AND i.index_id = s.index_id
WHERE OBJECTPROPERTY(i.object_id, 'IsUserTable') = 1
    AND s.user_seeks + s.user_scans + s.user_lookups = 0
ORDER BY s.user_updates DESC;
```

### Monitor Blocking

```sql
-- Check for blocking
SELECT 
    blocking_session_id,
    session_id,
    wait_type,
    wait_time,
    wait_resource
FROM sys.dm_exec_requests
WHERE blocking_session_id <> 0;
```

### EF Core Logging

```csharp
// Enable sensitive data logging in development
optionsBuilder.EnableSensitiveDataLogging();
optionsBuilder.EnableDetailedErrors();

// Log to console
optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);

// Log only SQL
optionsBuilder.LogTo(
    Console.WriteLine,
    new[] { DbLoggerCategory.Database.Command.Name },
    LogLevel.Information);
```

## Best Practices Summary

### DO
Use appropriate indexes
Use NoTracking for read-only queries
Use projections instead of loading full entities
Implement proper error handling
Use transactions for multi-step operations
Monitor query performance regularly
Use parameterized queries
Implement audit logging
Use connection pooling
Keep statistics up to date

### DON'T
Use SELECT *
Create indexes on every column
Ignore execution plans
Use EF for bulk operations (use Dapper or SQL bulk insert)
Keep long-running transactions
Forget to handle concurrency conflicts
Store sensitive data unencrypted
Grant excessive permissions
Ignore connection string security
Load entire tables into memory
