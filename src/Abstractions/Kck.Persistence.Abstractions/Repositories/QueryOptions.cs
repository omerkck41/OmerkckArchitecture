namespace Kck.Persistence.Abstractions.Repositories;

/// <summary>
/// Immutable query behavior toggles for repository read operations.
/// Provider-specific overloads may accept this record to reduce
/// positional-bool noise at call sites.
/// </summary>
/// <param name="IncludeDeleted">Include rows where <c>ISoftDeletable.IsDeleted</c> is <c>true</c>.</param>
/// <param name="AsTracking">Attach returned entities to the change tracker.</param>
public readonly record struct QueryOptions(bool IncludeDeleted = false, bool AsTracking = false)
{
    /// <summary>Default options: no tracking, no soft-deleted rows.</summary>
    public static QueryOptions None { get; }

    /// <summary>Options preset that enables change tracking.</summary>
    public static QueryOptions Tracking { get; } = new(AsTracking: true);

    /// <summary>Options preset that includes soft-deleted rows.</summary>
    public static QueryOptions WithDeleted { get; } = new(IncludeDeleted: true);
}
