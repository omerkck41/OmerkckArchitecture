namespace Kck.FeatureFlags.InMemory;

public sealed class InMemoryFeatureFlagOptions
{
    public Dictionary<string, bool> Features { get; set; } = new();
}
