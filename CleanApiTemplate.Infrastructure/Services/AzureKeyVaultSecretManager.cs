using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CleanApiTemplate.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CleanApiTemplate.Infrastructure.Services;

/// <summary>
/// Azure KeyVault implementation for secret management
/// Demonstrates secure secret storage and retrieval
/// </summary>
public class AzureKeyVaultSecretManager : ISecretManager
{
    private readonly SecretClient? _secretClient;
    private readonly bool _isEnabled;

    public AzureKeyVaultSecretManager(IConfiguration configuration)
    {
        var keyVaultUrl = configuration["AzureKeyVault:VaultUri"];

        if (!string.IsNullOrEmpty(keyVaultUrl))
        {
            // Use DefaultAzureCredential which works with:
            // - Visual Studio
            // - Azure CLI
            // - Managed Identity (when deployed to Azure)
            // - Environment variables
            _secretClient = new SecretClient(
                new Uri(keyVaultUrl),
                new DefaultAzureCredential());
            _isEnabled = true;
        }
        else
        {
            _isEnabled = false;
        }
    }

    public async Task<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_isEnabled || _secretClient == null)
        {
            return null;
        }

        try
        {
            var secret = await _secretClient.GetSecretAsync(key, cancellationToken: cancellationToken);
            return secret.Value.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            // Secret not found
            return null;
        }
    }

    public async Task SetSecretAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        if (!_isEnabled || _secretClient == null)
        {
            throw new InvalidOperationException("Azure KeyVault is not configured");
        }

        await _secretClient.SetSecretAsync(key, value, cancellationToken);
    }

    public async Task DeleteSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_isEnabled || _secretClient == null)
        {
            throw new InvalidOperationException("Azure KeyVault is not configured");
        }

        var operation = await _secretClient.StartDeleteSecretAsync(key, cancellationToken);
        await operation.WaitForCompletionAsync(cancellationToken);
    }
}
