using Kck.Core.Abstractions.Paging;
using Microsoft.EntityFrameworkCore;

namespace Kck.Persistence.EntityFramework.Paging;

/// <summary>
/// Async pagination extension methods for <see cref="IQueryable{T}"/> using EF Core.
/// </summary>
public static class QueryablePaginateExtensions
{
    /// <summary>
    /// Asynchronously paginates the <see cref="IQueryable{T}"/> source.
    /// </summary>
    public static async Task<IPaginate<T>> ToPaginateAsync<T>(
        this IQueryable<T> source,
        int index,
        int size,
        int from = 0,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(from);
        ArgumentOutOfRangeException.ThrowIfNegative(size);
        if (from > index)
            throw new ArgumentException(
                $"Index ({index}) is lower than From ({from}): pages are counted from From, so pass an index of at least {from}.",
                nameof(index));

        // (index - from) * size used to overflow into a negative OFFSET for a large client-supplied index.
        var offset = (long)(index - from) * size;
        if (offset > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(index), index, $"Page {index} of size {size} is beyond the largest supported offset.");

        var count = await source.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await source
            .Skip((int)offset)
            .Take(size)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Paginate<T>.Create(items, count, index, size, from);
    }
}
