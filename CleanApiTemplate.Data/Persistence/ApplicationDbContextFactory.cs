using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CleanApiTemplate.Data.Persistence;

/// <summary>
/// Design-time factory for ApplicationDbContext
/// Used by EF Core tools for migrations
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Get the current directory (CleanApiTemplate.Data project folder)
        var currentDir = Directory.GetCurrentDirectory();

        // Navigate to the API project folder (sibling directory)
        var apiProjectPath = Path.Combine(currentDir, "..", "CleanApiTemplate.API");

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        // Get connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        // Create DbContextOptions
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        ApplicationDbContext.ConfigureDbContext(optionsBuilder, connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}