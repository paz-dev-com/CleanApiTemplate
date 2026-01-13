using System.Diagnostics.CodeAnalysis;
using CleanApiTemplate.Core.Entities;
using CleanApiTemplate.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanApiTemplate.Data.Seeders;

/// <summary>
/// Example seeder for default categories
/// Demonstrates how to add additional seeders
/// </summary>
public class CategorySeeder(ILogger<CategorySeeder> logger) : ISeeder<ApplicationDbContext>
{
    private readonly ILogger<CategorySeeder> _logger = logger;

    public async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        // Check if categories already exist
        if (await context.Categories.AnyAsync(cancellationToken))
        {
            _logger.LogSeederSkip("categories");
            return;
        }

        _logger.LogSeederStart("categories");

        var categories = new List<Category>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Electronics",
                Description = "Electronic devices and accessories",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Books",
                Description = "Books and publications",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Clothing",
                Description = "Apparel and fashion items",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                IsDeleted = false
            }
        };

        await context.Categories.AddRangeAsync(categories, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogSeederComplete("categories", categories.Count);
    }
}
