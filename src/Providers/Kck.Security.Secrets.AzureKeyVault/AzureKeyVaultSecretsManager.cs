using System.Text.Json;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Kck.Security.Abstractions.Secrets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kck.Security.Secrets.AzureKeyVault;

/// <summary>
/// Retrieves and stores secrets in Azure Key Vault, by default through a <see cref="SecretClient"/> on
/// <see cref="DefaultAzureCredential"/>. Supports optional key prefixing to isolate secrets across environments or
/// tenants.
/// </summary>
public sealed partial class AzureKeyVaultSecretsManager : ISecretsManager
{
    private readonly SecretClient _client;
    private readonly ILogger<AzureKeyVaultSecretsManager> _logger;
    private readonly string? _prefix;

    /// <summary>
    /// Initializes the manager with a <see cref="SecretClient"/> for <see cref="AzureKeyVaultOptions.VaultUri"/>
    /// authenticated by <see cref="DefaultAzureCredential"/>.
    /// </summary>
    public AzureKeyVaultSecretsManager(
        IOptionsMonitor<AzureKeyVaultOptions> options,
        ILogger<AzureKeyVaultSecretsManager> logger)
        : this(options, logger, new SecretClient(new Uri(options.CurrentValue.VaultUri), new DefaultAzureCredential()))
    {
    }

    /// <summary>
    /// Initializes the manager with an existing <see cref="SecretClient"/>, for example one registered in the
    /// container with its own credential or client options; <see cref="AzureKeyVaultOptions.VaultUri"/> is then not
    /// used, <see cref="AzureKeyVaultOptions.SecretPrefix"/> still applies.
    /// </summary>
    public AzureKeyVaultSecretsManager(
        IOptionsMonitor<AzureKeyVaultOptions> options,
        ILogger<AzureKeyVaultSecretsManager> logger,
        SecretClient client)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
        _logger = logger;
        _prefix = options.CurrentValue.SecretPrefix;
    }

    /// <summary>
    /// Retrieves the raw string value of the secret identified by <paramref name="key"/>, or returns <see langword="null"/> if the secret does not exist.
    /// </summary>
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

    /// <summary>
    /// Retrieves the secret for <paramref name="key"/> and deserializes its JSON value to <typeparamref name="T"/>, returning <see langword="default"/> if not found.
    /// </summary>
    public async Task<T?> GetSecretAsync<T>(string key, CancellationToken ct = default)
    {
        var value = await GetSecretAsync(key, ct).ConfigureAwait(false);
        if (value is null) return default;

        if (typeof(T) == typeof(string))
        {
            // Boxing-based generic specialization: there is no implicit
            // conversion from string to T, so we go through System.Object.
            object boxed = value;
            return (T)boxed;
        }

        return JsonSerializer.Deserialize<T>(value);
    }

    /// <summary>Creates or updates the secret identified by <paramref name="key"/> in Azure Key Vault and logs the operation at Information level.</summary>
    public async Task SetSecretAsync(string key, string value, CancellationToken ct = default)
    {
        await _client.SetSecretAsync(BuildKey(key), value, ct).ConfigureAwait(false);
        LogSecretUpdated(_logger, key);
    }

    /// <summary>Returns <see langword="true"/> if a secret with the given <paramref name="key"/> exists in Azure Key Vault.</summary>
    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        return await GetSecretAsync(key, ct).ConfigureAwait(false) is not null;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Secret {Key} updated in Azure Key Vault")]
    private static partial void LogSecretUpdated(ILogger logger, string key);

    private string BuildKey(string key) =>
        string.IsNullOrEmpty(_prefix) ? key : $"{_prefix}-{key}";
}
