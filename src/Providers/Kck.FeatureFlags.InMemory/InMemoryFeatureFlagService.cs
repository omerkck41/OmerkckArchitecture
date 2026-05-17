using System.Collections.Concurrent;
using Kck.FeatureFlags.Abstractions;
using Microsoft.Extensions.Options;

namespace Kck.FeatureFlags.InMemory;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IFeatureFlagService"/> that reads its
/// initial feature state from <see cref="InMemoryFeatureFlagOptions"/> and stores it in a
/// <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// </summary>
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

    /// <summary>
    /// Returns <see langword="true"/> when the feature identified by <paramref name="featureName"/>
    /// is present in the dictionary and its <see cref="FeatureDefinition.Enabled"/> flag is set.
    /// </summary>
    public Task<bool> IsEnabledAsync(string featureName, CancellationToken ct = default)
    {
        var enabled = _features.TryGetValue(featureName, out var feature) && feature.Enabled;
        return Task.FromResult(enabled);
    }

    /// <summary>
    /// Context-aware overload that delegates to the context-free
    /// <see cref="IsEnabledAsync(string, CancellationToken)"/> because this in-memory provider
    /// does not perform per-user or per-tenant evaluation.
    /// </summary>
    public Task<bool> IsEnabledAsync(string featureName, IFeatureContext context, CancellationToken ct = default)
    {
        return IsEnabledAsync(featureName, ct);
    }

    /// <summary>
    /// Always returns <paramref name="defaultValue"/> because this provider does not store
    /// typed feature values.
    /// </summary>
    public Task<T> GetValueAsync<T>(string featureName, T defaultValue, CancellationToken ct = default)
    {
        return Task.FromResult(defaultValue);
    }

    /// <summary>
    /// Returns a read-only snapshot of all <see cref="FeatureDefinition"/> entries currently
    /// held in the in-memory store.
    /// </summary>
    public Task<IReadOnlyList<FeatureDefinition>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<FeatureDefinition> result = _features.Values.ToList().AsReadOnly();
        return Task.FromResult(result);
    }
}
