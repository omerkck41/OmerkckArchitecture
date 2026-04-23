using System.Text.Json;
using Kck.Security.Abstractions.Secrets;
using Microsoft.Extensions.Configuration;

namespace Kck.Security.Secrets.UserSecrets;

/// <summary>
/// ISecretsManager implementation backed by IConfiguration.
/// Reads from environment variables, User Secrets, appsettings.json, etc.
/// This is a read-only provider — SetSecretAsync is not supported.
/// </summary>
public sealed class ConfigurationSecretsManager(IConfiguration configuration) : ISecretsManager
{
    public Task<string?> GetSecretAsync(string key, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var value = configuration[key];
        return Task.FromResult(value);
    }

    public Task<T?> GetSecretAsync<T>(string key, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var value = configuration[key];

        if (value is null)
            return Task.FromResult(default(T));

        var result = JsonSerializer.Deserialize<T>(value);
        return Task.FromResult(result);
    }

    public Task SetSecretAsync(string key, string value, CancellationToken ct = default)
    {
        throw new NotSupportedException(
            "ConfigurationSecretsManager is read-only. " +
            "Use 'dotnet user-secrets set' CLI or environment variables to manage secrets.");
    }

    public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var value = configuration[key];
        return Task.FromResult(value is not null);
    }
}
