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
        if (from > index)
            throw new ArgumentException($"From ({from}) > Index ({index}), must From <= Index");

        var count = await source.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await source
            .Skip((index - from) * size)
            .Take(size)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Paginate<T>.Create(items, count, index, size, from);
    }
}
