using CleanApiTemplate.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CleanApiTemplate.Infrastructure.Services;

/// <summary>
/// Health check service implementation
/// Provides database connectivity checks
/// </summary>
public class HealthCheckService : IHealthCheckService
{
    private readonly DbContext _context;

    public HealthCheckService(DbContext context)
    {
        _context = context;
    }

    public async Task<bool> CheckDatabaseHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Database.CanConnectAsync(cancellationToken);
        }
        catch
        {
            return false;
        }
    }
}
