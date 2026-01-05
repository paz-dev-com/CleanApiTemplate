namespace CleanApiTemplate.Core.Interfaces;

/// <summary>
/// Interface for system interoperability operations
/// Provides access to Windows Registry, PowerShell, and system services
/// </summary>
public interface ISystemInteropService
{
    /// <summary>
    /// Read a value from Windows Registry
    /// </summary>
    /// <param name="keyPath">Registry key path (e.g., "HKEY_LOCAL_MACHINE\\Software\\...")</param>
    /// <param name="valueName">Name of the value to read</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Registry value or null if not found</returns>
    Task<object?> ReadRegistryValueAsync(
        string keyPath,
        string valueName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Write a value to Windows Registry
    /// </summary>
    /// <param name="keyPath">Registry key path</param>
    /// <param name="valueName">Name of the value to write</param>
    /// <param name="value">Value to write</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    Task WriteRegistryValueAsync(
        string keyPath,
        string valueName,
        object value,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a PowerShell script
    /// </summary>
    /// <param name="script">PowerShell script content</param>
    /// <param name="parameters">Script parameters</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Script output</returns>
    Task<string> ExecutePowerShellScriptAsync(
        string script,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a Windows service exists
    /// </summary>
    /// <param name="serviceName">Name of the service</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>True if service exists, false otherwise</returns>
    Task<bool> ServiceExistsAsync(string serviceName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Start a Windows service
    /// </summary>
    /// <param name="serviceName">Name of the service</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    Task StartServiceAsync(string serviceName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop a Windows service
    /// </summary>
    /// <param name="serviceName">Name of the service</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    Task StopServiceAsync(string serviceName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the status of a Windows service
    /// </summary>
    /// <param name="serviceName">Name of the service</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Service status (Running, Stopped, etc.)</returns>
    Task<string> GetServiceStatusAsync(string serviceName, CancellationToken cancellationToken = default);
}