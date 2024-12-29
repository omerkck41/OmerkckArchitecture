using Microsoft.EntityFrameworkCore;

namespace Core.Persistence.Paging;

public static class IQueryablePaginateExtensions
{
    public static async Task<IPaginate<T>> ToPaginateAsync<T>(this IQueryable<T> source, int index, int size, int from = 0,
                                                              CancellationToken cancellationToken = default)
    {
        if (from > index) throw new ArgumentException($"From: {from} > Index: {index}, must from <= Index");

        source = PreProcessQuery(source); // Ön işleme

        int count = await source.CountAsync(cancellationToken).ConfigureAwait(false);
        List<T> items = await source.Skip((index - from) * size).Take(size).ToListAsync(cancellationToken)
                                    .ConfigureAwait(false);
        return new Paginate<T>(items, index, size, from)
        {
            TotalRecords = count
        };
    }

    private static IQueryable<T> PreProcessQuery<T>(IQueryable<T> query)
    {
        // Gerekirse sıralama veya filtreleme eklenebilir
        return query;
    }

    public static IPaginate<T> ToPaginate<T>(this IQueryable<T> source, int index, int size, int from = 0)
    {
        if (from > index) throw new ArgumentException($"From: {from} > Index: {index}, must from <= Index");

        int count = source.Count();
        List<T> items = [.. source.Skip((index - from) * size).Take(size)];
        return new Paginate<T>(items, index, size, from)
        {
            TotalRecords = count
        };
    }
}