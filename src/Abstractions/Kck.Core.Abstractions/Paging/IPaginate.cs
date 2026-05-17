namespace Kck.Core.Abstractions.Paging;

/// <summary>
/// Read-only view of a paginated result set. Produced by <c>Paginate&lt;T&gt;.Create</c>
/// or EF Core extension <c>ToPaginateAsync</c>.
/// </summary>
/// <typeparam name="T">The type of items in this page.</typeparam>
public interface IPaginate<T>
{
    /// <summary>Zero-based starting page index (default: 0).</summary>
    int From { get; }

    /// <summary>Current page index (zero-based).</summary>
    int Index { get; }

    /// <summary>Maximum number of items per page.</summary>
    int Size { get; }

    /// <summary>Total number of items across all pages.</summary>
    int Count { get; }

    /// <summary>Total number of pages.</summary>
    int Pages { get; }

    /// <summary>Items on the current page.</summary>
    IReadOnlyList<T> Items { get; }

    /// <summary><see langword="true"/> when there is a page before the current one.</summary>
    bool HasPrevious { get; }

    /// <summary><see langword="true"/> when there is a page after the current one.</summary>
    bool HasNext { get; }

    /// <summary>Total number of records in the full data set.</summary>
    int TotalRecords { get; }

    /// <summary><see langword="true"/> when this is the first page.</summary>
    bool IsFirstPage { get; }

    /// <summary><see langword="true"/> when this is the last page.</summary>
    bool IsLastPage { get; }
}
