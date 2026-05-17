namespace Kck.FeatureFlags.InMemory;

/// <summary>
/// Configuration options for the in-memory feature flag provider.
/// Maps feature names to their enabled/disabled state via appsettings.
/// </summary>
public sealed class InMemoryFeatureFlagOptions
{
    /// <summary>
    /// Gets or sets the dictionary of feature names to boolean enabled flags loaded from
    /// configuration (e.g. appsettings.json).
    /// </summary>
    public Dictionary<string, bool> Features { get; set; } = new();
}
