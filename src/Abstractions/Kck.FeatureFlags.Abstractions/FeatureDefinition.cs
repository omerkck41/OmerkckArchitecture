namespace Kck.FeatureFlags.Abstractions;

/// <summary>
/// Describes a named feature flag including its enabled state, filter strategy, and optional configuration parameters.
/// </summary>
public sealed record FeatureDefinition
{
    /// <summary>Gets the unique name that identifies this feature flag.</summary>
    public required string Name { get; init; }

    /// <summary>Gets a value indicating whether the feature is globally enabled.</summary>
    public bool Enabled { get; init; }

    /// <summary>Gets the filter strategy used to conditionally enable the feature for a subset of users or contexts.</summary>
    public FeatureFilterType FilterType { get; init; } = FeatureFilterType.None;

    /// <summary>Gets optional key-value parameters passed to the selected filter strategy.</summary>
    public IReadOnlyDictionary<string, object>? Parameters { get; init; }
}

/// <summary>
/// Defines the strategy used to conditionally evaluate whether a feature is active for a given context.
/// </summary>
public enum FeatureFilterType
{
    /// <summary>No filter; the feature is evaluated solely by its <see cref="FeatureDefinition.Enabled"/> flag.</summary>
    None,

    /// <summary>Feature is enabled for a configured percentage of users.</summary>
    Percentage,

    /// <summary>Feature is enabled for specific user segments or groups.</summary>
    UserSegment,

    /// <summary>Feature is enabled only within a configured date/time range.</summary>
    DateRange,

    /// <summary>Feature is evaluated by a custom, user-defined filter implementation.</summary>
    Custom
}
