namespace Kck.FeatureFlags.Abstractions;

/// <summary>
/// Application-level service for querying feature flags, optionally scoped to a user/tenant context, and retrieving typed flag values.
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>Returns <see langword="true"/> if the named feature is globally enabled.</summary>
    Task<bool> IsEnabledAsync(string featureName, CancellationToken ct = default);

    /// <summary>Returns <see langword="true"/> if the named feature is enabled for the given <paramref name="context"/>.</summary>
    Task<bool> IsEnabledAsync(string featureName, IFeatureContext context, CancellationToken ct = default);

    /// <summary>Returns the typed configuration value of a feature flag, falling back to <paramref name="defaultValue"/> when the flag is absent.</summary>
    Task<T> GetValueAsync<T>(string featureName, T defaultValue, CancellationToken ct = default);

    /// <summary>Returns the complete list of all registered feature definitions.</summary>
    Task<IReadOnlyList<FeatureDefinition>> GetAllAsync(CancellationToken ct = default);
}
