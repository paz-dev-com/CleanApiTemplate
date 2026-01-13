using CleanApiTemplate.Core.Entities;
using CleanApiTemplate.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanApiTemplate.Data.Seeders;

/// <summary>
/// Seeder for default roles
/// Seeds roles with random GUIDs per environment
/// </summary>
public class RoleSeeder(ILogger<RoleSeeder> logger) : ISeeder<ApplicationDbContext>
{
    private readonly ILogger<RoleSeeder> _logger = logger;

    public async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        // Check if roles already exist
        if (await context.Roles.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Roles already exist, skipping seed");
            return;
        }

        _logger.LogInformation("Seeding default roles...");

        var roles = new List<Role>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                NormalizedName = "ADMIN",
                Description = "Administrator with full access",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "User",
                NormalizedName = "USER",
                Description = "Regular user with limited access",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Manager",
                NormalizedName = "MANAGER",
                Description = "Manager with elevated access",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                IsDeleted = false
            }
        };

        await context.Roles.AddRangeAsync(roles, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully seeded {Count} roles", roles.Count);
    }
}
