using System.Diagnostics;

namespace Kck.Core.Abstractions.Paging;

[DebuggerDisplay("Page {Index}/{Pages}, Items: {Items.Count}, Total: {TotalRecords}")]
public class Paginate<T> : IPaginate<T>
{
    public Paginate()
    {
        Items = Array.Empty<T>();
    }

    public int From { get; init; }
    public int Index { get; init; }
    public int Size { get; init; }
    public int Count { get; init; }
    public int Pages { get; init; }
    public IReadOnlyList<T> Items { get; init; }
    public bool HasPrevious { get; init; }
    public bool HasNext { get; init; }
    public int TotalRecords { get; init; }
    public bool IsFirstPage { get; init; }
    public bool IsLastPage { get; init; }

    /// <summary>
    /// Builds a <see cref="Paginate{T}"/> from pre-materialized counts and items.
    /// Provider-specific async factories (e.g., EF Core <c>ToPaginateAsync</c>)
    /// use this to avoid duplicating the derived-field math.
    /// </summary>
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
