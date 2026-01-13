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
public class IntegrationTestDbFactory : IAsyncLifetime, IAsyncDisposable
{
    private ApplicationDbContext? _dbContext;
    private DbConnection? _dbConnection;
    private Respawner? _respawner;

    public IntegrationTestDbFactory()
    {
        // First check for environment variable (used in CI/CD)
        string? envConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__TestConnection");
        
        if (!string.IsNullOrEmpty(envConnectionString))
        {
            ConnectionString = envConnectionString;
        }
        else
        {
            // Fall back to configuration file (for local development)
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: true)
                .AddJsonFile("../../../appsettings.Test.json", optional: true)
                .Build();

            ConnectionString = configuration.GetConnectionString("TestConnection")
                ?? "Server=localhost;Database=CleanApiTemplate_Test;User ID=sa;Password=P@ssw0rd;TrustServerCertificate=True;MultipleActiveResultSets=True;Connection Timeout=30;Encrypt=False;";
        }
    }

    /// <summary>
    /// Initialize test database
    /// </summary>
    public async Task InitializeAsync()
    {
        // Create DbContext
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        _dbContext = new ApplicationDbContext(options);

        try
        {
            // Ensure database is created and migrations are applied
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.MigrateAsync();

            // Initialize Respawner for database cleanup
            _dbConnection = new SqlConnection(ConnectionString);
            await _dbConnection.OpenAsync();

            _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
                TablesToIgnore = ["__EFMigrationsHistory"],
                WithReseed = true
            });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to initialize test database. Connection string: {ConnectionString}. " +
                $"Make sure SQL Server is running and accessible. Error: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Get a new DbContext instance for tests
    /// </summary>
    public ApplicationDbContext CreateDbContext()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString)
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
        using ApplicationDbContext context = CreateDbContext();
        context.Set<T>().AddRange(entities);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seed test data - accepts multiple entity types
    /// </summary>
    public async Task SeedEntitiesAsync(params object[] entities)
    {
        using ApplicationDbContext context = CreateDbContext();
        foreach (object entity in entities)
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
        using ApplicationDbContext context = CreateDbContext();
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    /// <summary>
    /// Cleanup resources
    /// </summary>
    public async ValueTask DisposeAsync()
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

        GC.SuppressFinalize(this);
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get connection string for tests
    /// </summary>
    public string ConnectionString { get; }
}
