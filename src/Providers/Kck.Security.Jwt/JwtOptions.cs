namespace Kck.Security.Jwt;

/// <summary>JWT token configuration. Key must be provided — no fallback allowed.</summary>
public sealed class JwtOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public TimeSpan AccessTokenExpiration { get; set; } = TimeSpan.FromMinutes(15);
    public int RefreshTokenTtlDays { get; set; } = 7;
    public RsaKeySource KeySource { get; set; } = RsaKeySource.Configuration;
    public string? RsaKeyPath { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public string? RsaKeyBase64 { get; set; }
}

/// <summary>How the RSA signing key is loaded.</summary>
public enum RsaKeySource
{
    /// <summary>Base64-encoded key from configuration/env variable.</summary>
    Configuration,
    /// <summary>PEM file on disk.</summary>
    File,
    /// <summary>Never implemented: options validation rejects it. Load the key yourself and set <c>RsaKeyBase64</c>.</summary>
    [Obsolete(
        "Never implemented: the key is not loaded from ISecretsManager. Read the key yourself (for example from " +
        "ISecretsManager or Key Vault) and set RsaKeyBase64 with RsaKeySource.Configuration. Removed in v5.0.",
        DiagnosticId = "KCK0300",
        UrlFormat = "https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/policies/deprecation.md#{0}")]
    SecretsManager
}
