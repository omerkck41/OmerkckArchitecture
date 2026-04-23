namespace Kck.Search.Abstractions;

/// <summary>
/// Paged search result.
/// </summary>
public sealed class SearchResult<T> where T : class
{
    /// <summary>Matching documents for the current page.</summary>
    public required IReadOnlyList<T> Items { get; init; }

    /// <summary>Total number of matching documents across all pages.</summary>
    public long TotalCount { get; init; }
}
