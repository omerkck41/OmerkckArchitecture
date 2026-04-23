namespace Kck.Security.Abstractions.Secrets;

/// <summary>
/// Provider-agnostic secret storage (env variables, User Secrets, Azure Key Vault, etc.).
/// </summary>
public interface ISecretsManager
{
    /// <summary>Retrieves a secret by key. Returns null if not found.</summary>
    Task<string?> GetSecretAsync(string key, CancellationToken ct = default);

    /// <summary>Retrieves and deserializes a secret.</summary>
    Task<T?> GetSecretAsync<T>(string key, CancellationToken ct = default);

    /// <summary>Stores a secret.</summary>
    Task SetSecretAsync(string key, string value, CancellationToken ct = default);

    /// <summary>Checks if a secret exists.</summary>
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);
}
