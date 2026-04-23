namespace Kck.FeatureFlags.Abstractions;

public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(string featureName, CancellationToken ct = default);
    Task<bool> IsEnabledAsync(string featureName, IFeatureContext context, CancellationToken ct = default);
    Task<T> GetValueAsync<T>(string featureName, T defaultValue, CancellationToken ct = default);
    Task<IReadOnlyList<FeatureDefinition>> GetAllAsync(CancellationToken ct = default);
}
