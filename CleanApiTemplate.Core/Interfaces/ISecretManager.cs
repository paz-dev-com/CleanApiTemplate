namespace CleanApiTemplate.Core.Interfaces;

/// <summary>
/// Interface for secure secret management
/// Abstracts access to secrets from Azure KeyVault or other secret stores
/// </summary>
public interface ISecretManager
{
    /// <summary>
    /// Get a secret value by key asynchronously
    /// </summary>
    /// <param name="key">Secret key/name</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Secret value or null if not found</returns>
    Task<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set or update a secret value asynchronously
    /// </summary>
    /// <param name="key">Secret key/name</param>
    /// <param name="value">Secret value</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    Task SetSecretAsync(string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a secret asynchronously
    /// </summary>
    /// <param name="key">Secret key/name</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    Task DeleteSecretAsync(string key, CancellationToken cancellationToken = default);
}