using System.Collections.Concurrent;
using Kck.FeatureFlags.Abstractions;
using Microsoft.Extensions.Options;

namespace Kck.FeatureFlags.InMemory;

public sealed class InMemoryFeatureFlagService(
    IOptionsMonitor<InMemoryFeatureFlagOptions> options) : IFeatureFlagService
{
    private readonly ConcurrentDictionary<string, FeatureDefinition> _features = new(
        options.CurrentValue.Features.Select(kvp =>
            KeyValuePair.Create(kvp.Key, new FeatureDefinition
            {
                Name = kvp.Key,
                Enabled = kvp.Value
            })));

    public Task<bool> IsEnabledAsync(string featureName, CancellationToken ct = default)
    {
        var enabled = _features.TryGetValue(featureName, out var feature) && feature.Enabled;
        return Task.FromResult(enabled);
    }

    public Task<bool> IsEnabledAsync(string featureName, IFeatureContext context, CancellationToken ct = default)
    {
        return IsEnabledAsync(featureName, ct);
    }

    public Task<T> GetValueAsync<T>(string featureName, T defaultValue, CancellationToken ct = default)
    {
        return Task.FromResult(defaultValue);
    }

    public Task<IReadOnlyList<FeatureDefinition>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<FeatureDefinition> result = _features.Values.ToList().AsReadOnly();
        return Task.FromResult(result);
    }
}
