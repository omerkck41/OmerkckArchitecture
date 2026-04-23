namespace Kck.FeatureFlags.Abstractions;

public sealed record FeatureDefinition
{
    public required string Name { get; init; }
    public bool Enabled { get; init; }
    public FeatureFilterType FilterType { get; init; } = FeatureFilterType.None;
    public IReadOnlyDictionary<string, object>? Parameters { get; init; }
}

public enum FeatureFilterType
{
    None,
    Percentage,
    UserSegment,
    DateRange,
    Custom
}
