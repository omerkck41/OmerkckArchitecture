using System.Text.Json;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Kck.Security.Abstractions.Secrets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kck.Security.Secrets.AzureKeyVault;

public sealed partial class AzureKeyVaultSecretsManager(
    IOptionsMonitor<AzureKeyVaultOptions> options,
    ILogger<AzureKeyVaultSecretsManager> logger) : ISecretsManager
{
    private readonly SecretClient _client = new(
        new Uri(options.CurrentValue.VaultUri),
        new DefaultAzureCredential());
    private readonly string? _prefix = options.CurrentValue.SecretPrefix;

    public async Task<string?> GetSecretAsync(string key, CancellationToken ct = default)
    {
        try
        {
            var response = await _client.GetSecretAsync(BuildKey(key), cancellationToken: ct).ConfigureAwait(false);
            return response.Value.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<T?> GetSecretAsync<T>(string key, CancellationToken ct = default)
    {
        var value = await GetSecretAsync(key, ct).ConfigureAwait(false);
        if (value is null) return default;

        if (typeof(T) == typeof(string))
            return (T)(object)value;

        return JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetSecretAsync(string key, string value, CancellationToken ct = default)
    {
        await _client.SetSecretAsync(BuildKey(key), value, ct).ConfigureAwait(false);
        LogSecretUpdated(logger, key);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        return await GetSecretAsync(key, ct).ConfigureAwait(false) is not null;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Secret {Key} updated in Azure Key Vault")]
    private static partial void LogSecretUpdated(ILogger logger, string key);

    private string BuildKey(string key) =>
        string.IsNullOrEmpty(_prefix) ? key : $"{_prefix}-{key}";
}
