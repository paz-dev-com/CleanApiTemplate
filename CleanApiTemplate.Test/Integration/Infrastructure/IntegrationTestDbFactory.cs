using CleanApiTemplate.Data.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Respawn;
using System.Data.Common;

namespace CleanApiTemplate.Test.Integration.Infrastructure;

/// <summary>
/// Integration test database factory
/// Provides database context for integration tests using real SQL Server
/// </summary>
public class IntegrationTestDbFactory : IAsyncLifetime
{
    private readonly string _connectionString;
    private ApplicationDbContext? _dbContext;
    private DbConnection? _dbConnection;
    private Respawner? _respawner;

    public IntegrationTestDbFactory()
    {
        // First check for environment variable (used in CI/CD)
        var envConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__TestConnection");
        
        if (!string.IsNullOrEmpty(envConnectionString))
        {
            _connectionString = envConnectionString;
        }
        else
        {
            // Fall back to configuration file (for local development)
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: true)
                .AddJsonFile("../../../appsettings.Test.json", optional: true)
                .Build();

            _connectionString = configuration.GetConnectionString("TestConnection")
                ?? "Server=tcp:localhost,1433;Initial Catalog=CleanApiTemplate_Test;Persist Security Info=False;User ID=sa;Password=P@ssw0rd123!;MultipleActiveResultSets=True;Connection Timeout=30;TrustServerCertificate=True;";
        }
    }

    /// <summary>
    /// Initialize test database
    /// </summary>
    public async Task InitializeAsync()
    {
        // Create DbContext
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        _dbContext = new ApplicationDbContext(options);

        // Ensure database is created and migrations are applied
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.Database.MigrateAsync();

        // Initialize Respawner for database cleanup
        _dbConnection = new SqlConnection(_connectionString);
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            TablesToIgnore = ["__EFMigrationsHistory"],
            WithReseed = true
        });
    }

    /// <summary>
    /// Get a new DbContext instance for tests
    /// </summary>
    public ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Reset database to clean state between tests
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        if (_respawner != null && _dbConnection != null)
        {
            await _respawner.ResetAsync(_dbConnection);
        }
    }

    /// <summary>
    /// Seed test data - accepts array of entities of same type
    /// </summary>
    public async Task SeedAsync<T>(params T[] entities) where T : class
    {
        using var context = CreateDbContext();
        context.Set<T>().AddRange(entities);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seed test data - accepts multiple entity types
    /// </summary>
    public async Task SeedEntitiesAsync(params object[] entities)
    {
        using var context = CreateDbContext();
        foreach (var entity in entities)
        {
            context.Add(entity);
        }
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Execute raw SQL command
    /// </summary>
    public async Task ExecuteSqlAsync(string sql)
    {
        using var context = CreateDbContext();
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    /// <summary>
    /// Cleanup resources
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.DisposeAsync();
        }

        if (_dbConnection != null)
        {
            await _dbConnection.CloseAsync();
            await _dbConnection.DisposeAsync();
        }
    }

    /// <summary>
    /// Get connection string for tests
    /// </summary>
    public string ConnectionString => _connectionString;
}
