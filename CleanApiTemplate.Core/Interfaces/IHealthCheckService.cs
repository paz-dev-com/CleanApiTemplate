namespace CleanApiTemplate.Core.Interfaces;

/// <summary>
/// Interface for health check operations
/// Abstracts infrastructure health status checks
/// </summary>
public interface IHealthCheckService
{
    /// <summary>
    /// Check if the database connection is healthy
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>True if database is accessible, false otherwise</returns>
    Task<bool> CheckDatabaseHealthAsync(CancellationToken cancellationToken = default);
}
