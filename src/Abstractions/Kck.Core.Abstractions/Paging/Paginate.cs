using System.Diagnostics;

namespace Kck.Core.Abstractions.Paging;

/// <summary>
/// Concrete paginated result. Use <see cref="Create"/> to build from pre-materialised data,
/// or the EF Core <c>ToPaginateAsync</c> extension for database-backed pagination.
/// </summary>
/// <remarks>
/// <para>
/// Changed to <c>sealed record</c> in v3.0: supports <c>with</c> operator and structural
/// equality. Existing code unaffected unless it compares <see cref="Paginate{T}"/> instances
/// with <c>==</c> (now structural instead of reference equality).
/// </para>
/// </remarks>
/// <typeparam name="T">The type of items on this page.</typeparam>
[DebuggerDisplay("Page {Index}/{Pages}, Items: {Items.Count}, Total: {TotalRecords}")]
public sealed record Paginate<T> : IPaginate<T>
{
    /// <summary>Initialises an empty paginate with zero items.</summary>
    public Paginate()
    {
        Items = Array.Empty<T>();
    }

    /// <inheritdoc/>
    public int From { get; init; }
    /// <inheritdoc/>
    public int Index { get; init; }
    /// <inheritdoc/>
    public int Size { get; init; }
    /// <inheritdoc/>
    public int Count { get; init; }
    /// <inheritdoc/>
    public int Pages { get; init; }
    /// <inheritdoc/>
    public IReadOnlyList<T> Items { get; init; }
    /// <inheritdoc/>
    public bool HasPrevious { get; init; }
    /// <inheritdoc/>
    public bool HasNext { get; init; }
    /// <inheritdoc/>
    public int TotalRecords { get; init; }
    /// <inheritdoc/>
    public bool IsFirstPage { get; init; }
    /// <inheritdoc/>
    public bool IsLastPage { get; init; }

    /// <summary>
    /// Builds a <see cref="Paginate{T}"/> from pre-materialised counts and items.
    /// </summary>
    /// <param name="items">The items on the current page.</param>
    /// <param name="totalCount">Total number of items across all pages.</param>
    /// <param name="index">Zero-based current page index.</param>
    /// <param name="size">Maximum items per page.</param>
    /// <param name="from">Zero-based starting page offset (default: 0).</param>
#pragma warning disable CA1000
    public static Paginate<T> Create(
        IReadOnlyList<T> items,
        int totalCount,
        int index,
        int size,
        int from = 0)
    {
        if (from > index)
            throw new ArgumentException($"From ({from}) > Index ({index}), must From <= Index");

        var pages = size > 0 ? (int)Math.Ceiling(totalCount / (double)size) : 0;
        return new Paginate<T>
        {
            From = from,
            Index = index,
            Size = size,
            Count = totalCount,
            TotalRecords = totalCount,
            Items = items,
            Pages = pages,
            HasPrevious = index - from > 0,
            HasNext = index - from + 1 < pages,
            IsFirstPage = index - from == 0,
            IsLastPage = index - from + 1 >= pages
        };
    }
#pragma warning restore CA1000
}
