using CleanApiTemplate.Core.Interfaces;
using Microsoft.Win32;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.Versioning;
using System.ServiceProcess;

namespace CleanApiTemplate.Infrastructure.Services;

/// <summary>
/// System interoperability service for Windows operations
/// Demonstrates Registry access, PowerShell execution, and Windows Service management
/// WARNING: These operations require appropriate permissions
/// </summary>
public class SystemInteropService : ISystemInteropService
{
    /// <summary>
    /// Read Windows Registry value
    /// </summary>
    [SupportedOSPlatform("windows")]
    public Task<object?> ReadRegistryValueAsync(
        string keyPath,
        string valueName,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                // Parse the registry hive and subkey
                var parts = keyPath.Split('\\', 2);
                if (parts.Length != 2)
                {
                    throw new ArgumentException("Invalid registry key path format", nameof(keyPath));
                }

                var hive = GetRegistryHive(parts[0]);
                var subKey = parts[1];

                using var key = hive?.OpenSubKey(subKey, false);
                return key?.GetValue(valueName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read registry value: {ex.Message}", ex);
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Write Windows Registry value
    /// </summary>
    [SupportedOSPlatform("windows")]
    public Task WriteRegistryValueAsync(
        string keyPath,
        string valueName,
        object value,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                var parts = keyPath.Split('\\', 2);
                if (parts.Length != 2)
                {
                    throw new ArgumentException("Invalid registry key path format", nameof(keyPath));
                }

                var hive = GetRegistryHive(parts[0]);
                var subKey = parts[1];

                using var key = hive?.OpenSubKey(subKey, true) ?? hive?.CreateSubKey(subKey);
                key?.SetValue(valueName, value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to write registry value: {ex.Message}", ex);
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Execute PowerShell script
    /// </summary>
    public Task<string> ExecutePowerShellScriptAsync(
        string script,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                // Create runspace
                using var runspace = RunspaceFactory.CreateRunspace();
                runspace.Open();

                // Create PowerShell instance
                using var powershell = PowerShell.Create();
                powershell.Runspace = runspace;

                // Add script
                powershell.AddScript(script);

                // Add parameters if provided
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        powershell.AddParameter(param.Key, param.Value);
                    }
                }

                // Execute script
                var results = powershell.Invoke();

                // Check for errors
                if (powershell.HadErrors)
                {
                    var errors = string.Join(Environment.NewLine,
                        powershell.Streams.Error.Select(e => e.ToString()));
                    throw new InvalidOperationException($"PowerShell script execution failed: {errors}");
                }

                // Return results as string
                return string.Join(Environment.NewLine, results.Select(r => r.ToString()));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to execute PowerShell script: {ex.Message}", ex);
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Check if Windows service exists
    /// </summary>
    [SupportedOSPlatform("windows")]
    public Task<bool> ServiceExistsAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                var services = ServiceController.GetServices();
                return services.Any(s => s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to check service existence: {ex.Message}", ex);
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Start Windows service
    /// </summary>
    [SupportedOSPlatform("windows")]
    public Task StartServiceAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                using var service = new ServiceController(serviceName);

                if (service.Status == ServiceControllerStatus.Running)
                {
                    return;
                }

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to start service: {ex.Message}", ex);
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Stop Windows service
    /// </summary>
    [SupportedOSPlatform("windows")]
    public Task StopServiceAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                using var service = new ServiceController(serviceName);

                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    return;
                }

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to stop service: {ex.Message}", ex);
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Get Windows service status
    /// </summary>
    [SupportedOSPlatform("windows")]
    public Task<string> GetServiceStatusAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                using var service = new ServiceController(serviceName);
                return service.Status.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get service status: {ex.Message}", ex);
            }
        }, cancellationToken);
    }

    [SupportedOSPlatform("windows")]
    private static RegistryKey? GetRegistryHive(string hiveName)
    {
        return hiveName.ToUpperInvariant() switch
        {
            "HKEY_CLASSES_ROOT" or "HKCR" => Registry.ClassesRoot,
            "HKEY_CURRENT_USER" or "HKCU" => Registry.CurrentUser,
            "HKEY_LOCAL_MACHINE" or "HKLM" => Registry.LocalMachine,
            "HKEY_USERS" or "HKU" => Registry.Users,
            "HKEY_CURRENT_CONFIG" or "HKCC" => Registry.CurrentConfig,
            _ => throw new ArgumentException($"Unknown registry hive: {hiveName}", nameof(hiveName))
        };
    }
}
