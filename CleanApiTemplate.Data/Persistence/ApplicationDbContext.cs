using CleanApiTemplate.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CleanApiTemplate.Data.Persistence;

/// <summary>
/// Main database context for the application
/// Demonstrates EF Core configuration and best practices
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    // DbSets for entities
    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Global query filter for soft delete
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Role>().HasQueryFilter(r => !r.IsDeleted);
        modelBuilder.Entity<UserRole>().HasQueryFilter(ur => !ur.IsDeleted);

        // Configure indexes for performance
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Sku)
            .IsUnique()
            .HasDatabaseName("IX_Products_Sku");

        modelBuilder.Entity<Product>()
            .HasIndex(p => new { p.CategoryId, p.IsActive })
            .HasDatabaseName("IX_Products_CategoryId_IsActive");

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Name)
            .HasDatabaseName("IX_Products_Name");

        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => new { a.EntityName, a.EntityId, a.PerformedAt })
            .HasDatabaseName("IX_AuditLogs_Entity_Date");

        // Roles will be seeded at application startup instead of using HasData
        // This allows for dynamic GUID generation per environment
    }

    /// <summary>
    /// Override SaveChanges to automatically set audit fields
    /// Demonstrates automatic audit trail implementation
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Get all modified entries
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added ||
                       e.State == EntityState.Modified ||
                       e.State == EntityState.Deleted);

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    // CreatedBy should be set by the handler using ICurrentUserService
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    // UpdatedBy should be set by the handler using ICurrentUserService
                    break;

                case EntityState.Deleted:
                    // Implement soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    // DeletedBy should be set by the handler using ICurrentUserService
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Configure SQL Server specific options for performance
    /// </summary>
    public static void ConfigureDbContext(DbContextOptionsBuilder options, string connectionString)
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            // Enable retry on failure for resilience
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);

            // Command timeout for long-running queries
            sqlOptions.CommandTimeout(30);

            // Use newer SQL Server compatibility level
            sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });

        // Enable sensitive data logging in development only
#if DEBUG
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
#endif
    }
}