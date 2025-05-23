﻿using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Core.Persistence.Paging;

public static class IQueryablePaginateExtensions
{
    public static async Task<IPaginate<T>> ToPaginateAsync<T>(this IQueryable<T> source, int index, int size, int from = 0,
                                                              CancellationToken cancellationToken = default)
    {
        if (from > index) throw new CustomArgumentException($"From: {from} > Index: {index}, must from <= Index");


        int count = await source.CountAsync(cancellationToken).ConfigureAwait(false);
        List<T> items = await source.Skip((index - from) * size).Take(size).ToListAsync(cancellationToken).ConfigureAwait(false);


        return new Paginate<T>()
        {
            Index = index,
            Size = size,
            From = from,
            Count = count,
            TotalRecords = count,
            Items = items,
            Pages = (int)Math.Ceiling(count / (double)size)
        };
    }

    public static IPaginate<T> ToPaginate<T>(this IQueryable<T> source, int index, int size, int from = 0)
    {
        if (from > index) throw new CustomArgumentException($"From: {from} > Index: {index}, must from <= Index");

        int count = source.Count();
        int skip = (index - from) * size;

        // Eğer skip değeri toplam kayıt sayısını aşarsa, maksimum kayıt noktasına çek
        skip = Math.Min(skip, count);

        List<T> items = source.Skip(skip).Take(size).ToList();

        return new Paginate<T>()
        {
            Index = index,
            Size = size,
            From = from,
            Count = count,
            TotalRecords = count,
            Items = items,
            Pages = (int)Math.Ceiling(count / (double)size)
        };
    }
}