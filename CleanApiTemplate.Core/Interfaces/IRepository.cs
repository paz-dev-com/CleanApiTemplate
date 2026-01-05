namespace CleanApiTemplate.Core.Interfaces;

/// <summary>
/// Generic repository interface defining common data access operations
/// Follows Repository pattern for data abstraction
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Get entity by ID asynchronously
    /// </summary>
    /// <param name="id">Entity identifier</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Entity or null if not found</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all entities asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Collection of entities</returns>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Find entities matching a predicate
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Filtered collection of entities</returns>
    Task<IEnumerable<T>> FindAsync(
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new entity asynchronously
    /// </summary>
    /// <param name="entity">Entity to add</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add multiple entities asynchronously
    /// </summary>
    /// <param name="entities">Entities to add</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing entity
    /// </summary>
    /// <param name="entity">Entity to update</param>
    void Update(T entity);

    /// <summary>
    /// Update multiple entities
    /// </summary>
    /// <param name="entities">Entities to update</param>
    void UpdateRange(IEnumerable<T> entities);

    /// <summary>
    /// Remove an entity (hard delete)
    /// </summary>
    /// <param name="entity">Entity to remove</param>
    void Remove(T entity);

    /// <summary>
    /// Remove multiple entities (hard delete)
    /// </summary>
    /// <param name="entities">Entities to remove</param>
    void RemoveRange(IEnumerable<T> entities);

    /// <summary>
    /// Check if any entity matches the predicate
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>True if any entity matches, false otherwise</returns>
    Task<bool> AnyAsync(
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Count entities matching a predicate
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Count of matching entities</returns>
    Task<int> CountAsync(
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);
}