using CleanApiTemplate.Core.Interfaces;
using CleanApiTemplate.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CleanApiTemplate.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure layer services
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Register infrastructure services
        services.AddSingleton<ISecretManager, AzureKeyVaultSecretManager>();
        services.AddSingleton<ICryptographyService, CryptographyService>();
        services.AddScoped<ISystemInteropService, SystemInteropService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        
        // Register Health Check Service (requires DbContext from Persistence layer)
        services.AddScoped<IHealthCheckService>(sp => 
            new HealthCheckService(sp.GetRequiredService<DbContext>()));

        return services;
    }
}
