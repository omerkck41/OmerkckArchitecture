namespace Kck.FeatureFlags.AzureAppConfig;

/// <summary>
/// Configuration for <see cref="AzureAppConfigFeatureFlagService"/>.
/// Provide either <see cref="ConnectionString"/> or <see cref="Endpoint"/> (Managed Identity).
/// </summary>
public sealed class AzureAppConfigOptions
{
    /// <summary>
    /// Azure App Configuration connection string.
    /// Mutually exclusive with <see cref="Endpoint"/>; prefer <see cref="Endpoint"/> for production
    /// to avoid storing secrets.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Azure App Configuration endpoint URL (e.g. <c>https://&lt;store&gt;.azconfig.io</c>).
    /// Used with DefaultAzureCredential (Managed Identity / local developer credentials).
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// Key prefix filter applied when loading feature flags.
    /// Only flags whose keys begin with this prefix are loaded.
    /// Default: <c>".appconfig.featureflag/"</c> (Azure standard prefix).
    /// </summary>
    public string KeyPrefix { get; set; } = ".appconfig.featureflag/";

    /// <summary>
    /// Label filter for feature flags. <see langword="null"/> loads flags with no label.
    /// Use <c>"\0"</c> to load all labels.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// How often the service re-fetches flags from Azure App Configuration.
    /// Default: 30 seconds.
    /// </summary>
    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromSeconds(30);
}
