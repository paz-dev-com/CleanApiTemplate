using Microsoft.EntityFrameworkCore;

namespace CleanApiTemplate.Data.Seeders;

/// <summary>
/// Interface for database seeders
/// Allows dynamic seeding of entities with DbContext access
/// </summary>
/// <typeparam name="TContext">The DbContext type</typeparam>
public interface ISeeder<in TContext> where TContext : DbContext
{
    /// <summary>
    /// Seed data into the database
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SeedAsync(TContext context, CancellationToken cancellationToken = default);
}
