namespace CleanApiTemplate.Core.Interfaces;

/// <summary>
/// Unit of Work pattern interface
/// Coordinates the work of multiple repositories and manages transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Get repository for a specific entity type
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <returns>Repository instance</returns>
    IRepository<T> Repository<T>() where T : class;

    /// <summary>
    /// Save all changes asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Number of entities affected</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begin a database transaction asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commit the current transaction asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback the current transaction asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute raw SQL query asynchronously (for performance-critical operations)
    /// </summary>
    /// <typeparam name="T">Result type</typeparam>
    /// <param name="sql">SQL query</param>
    /// <param name="parameters">Query parameters</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Query results</returns>
    Task<IEnumerable<T>> ExecuteQueryAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default);
}