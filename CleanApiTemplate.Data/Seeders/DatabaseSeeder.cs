using CleanApiTemplate.Data.Persistence;
using Microsoft.Extensions.Logging;

namespace CleanApiTemplate.Data.Seeders;

/// <summary>
/// Main database seeder that orchestrates all seeders
/// Discovers and executes all registered ISeeder implementations
/// </summary>
public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IEnumerable<ISeeder<ApplicationDbContext>> _seeders;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        ApplicationDbContext context,
        IEnumerable<ISeeder<ApplicationDbContext>> seeders,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _seeders = seeders;
        _logger = logger;
    }

    /// <summary>
    /// Execute all registered seeders
    /// </summary>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            foreach (var seeder in _seeders)
            {
                var seederName = seeder.GetType().Name;
                _logger.LogInformation("Executing seeder: {SeederName}", seederName);
                
                await seeder.SeedAsync(_context, cancellationToken);
            }

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}
