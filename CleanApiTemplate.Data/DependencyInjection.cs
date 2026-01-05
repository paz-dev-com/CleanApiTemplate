using CleanApiTemplate.Core.Interfaces;
using CleanApiTemplate.Data.Persistence;
using CleanApiTemplate.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanApiTemplate.Data;

/// <summary>
/// Extension methods for registering Persistence layer services
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        // Database context with connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            ApplicationDbContext.ConfigureDbContext(options, connectionString!));

        // Register repositories and Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Health Check Service (depends on DbContext)
        // HealthCheckService is now in Infrastructure layer, registered there

        return services;
    }
}
