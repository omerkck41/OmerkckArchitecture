﻿using Core.Persistence.Entities;
using Core.Persistence.Paging;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Core.Persistence.Repositories;

public interface IAsyncRepository<T> : IQuery<T> where T : Entity
{
    /// <summary>
    /// Tek bir varlık getirir.
    /// </summary>
    Task<T?> GetAsync(Expression<Func<T, bool>> predicate,
                      Func<IQueryable<T>, IIncludableQueryable<T, object>>[]? includes = null,
                      bool enableTracking = true,
                      CancellationToken cancellationToken = default);

    /// <summary>
    /// Liste döner, sayfalama ve sıralama destekler.
    /// </summary>
    Task<IPaginate<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null,
                                    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                    Func<IQueryable<T>, IIncludableQueryable<T, object>>[]? includes = null,
                                    int index = 0, int size = 10, bool enableTracking = true,
                                    CancellationToken cancellationToken = default);

    /// <summary>
    /// Dinamik sorgularla bir liste döner.
    /// </summary>
    Task<IPaginate<T>> GetListByDynamicAsync(Dynamic.Dynamic dynamic,
                                             Func<IQueryable<T>, IIncludableQueryable<T, object>>[]? includes = null,
                                             int index = 0, int size = 10, bool enableTracking = true,
                                             CancellationToken cancellationToken = default);


    Task<T?> GetByIdAsync(int id);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    /// <summary>
    /// Yeni bir varlık ekler ve eklenen varlığı döner.
    /// </summary>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Birden fazla varlık ekler.
    /// </summary>
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Bir varlığı günceller.
    /// </summary>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Tekil veya birden fazla alanı günceller.
    /// </summary>
    Task<T> UpdatePartialAsync(T entity, params Expression<Func<T, object>>[] properties);

    /// <summary>
    /// Toplu güncellemeler için kullanılır
    /// </summary>
    Task BulkUpdateAsync(Expression<Func<T, bool>> predicate, params (Expression<Func<T, object>> Property, object Value)[] updates);

    /// <summary>
    /// Birden fazla varlığı günceller.
    /// </summary>
    Task UpdateRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Bir varlığı siler.
    /// </summary>
    Task<T> DeleteAsync(T entity);

    /// <summary>
    /// Birden fazla varlığı siler.
    /// </summary>
    Task DeleteRangeAsync(IEnumerable<T> entities);

    Task<T> SoftDeleteAsync(T entity);
    Task SoftDeleteRangeAsync(IEnumerable<T> entities);
}