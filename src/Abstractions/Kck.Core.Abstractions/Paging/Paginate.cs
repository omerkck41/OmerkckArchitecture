using System.Diagnostics;

namespace Kck.Core.Abstractions.Paging;

[DebuggerDisplay("Page {Index}/{Pages}, Items: {Items.Count}, Total: {TotalRecords}")]
public class Paginate<T> : IPaginate<T>
{
    public Paginate()
    {
        Items = Array.Empty<T>();
    }

    public int From { get; set; }
    public int Index { get; set; }
    public int Size { get; set; }
    public int Count { get; set; }
    public int Pages { get; set; }
    public IReadOnlyList<T> Items { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
    public int TotalRecords { get; set; }
    public bool IsFirstPage { get; set; }
    public bool IsLastPage { get; set; }

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
