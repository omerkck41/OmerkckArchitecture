namespace Kck.Search.Abstractions;

/// <summary>
/// Represents a search query with paging and sorting.
/// </summary>
public sealed class SearchRequest
{
    /// <summary>Query string (Lucene syntax for full-text providers).</summary>
    public required string Query { get; init; }

    /// <summary>Index or collection name. Null uses provider default.</summary>
    public string? IndexName { get; init; }

    /// <summary>Zero-based offset for paging.</summary>
    public int From { get; init; }

    /// <summary>Page size.</summary>
    public int Size { get; init; } = 20;

    /// <summary>Sort field name. Null uses relevance score.</summary>
    public string? SortField { get; init; }

    /// <summary>Sort direction. True = ascending.</summary>
    public bool SortAscending { get; init; } = true;
}
