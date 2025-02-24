using Core.Persistence.Entities;
using Core.Persistence.Paging;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Core.Persistence.Repositories;

public interface IAsyncRepository<T, TId> : IQuery<T> where T : Entity<TId>
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
                                             Expression<Func<T, bool>>? predicate = null,
                                             Func<IQueryable<T>, IIncludableQueryable<T, object>>[]? includes = null,
                                             int index = 0, int size = 10, bool enableTracking = true,
                                             CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(
                        Expression<Func<T, bool>>? predicate = null,
                        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
                        bool enableTracking = false,
                        CancellationToken cancellationToken = default);

    Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    /// <summary>
    /// Yeni bir varlık ekler ve eklenen varlığı döner.
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Birden fazla varlık ekler.
    /// </summary>
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bir varlığı günceller.
    /// </summary>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tekil veya birden fazla alanı günceller.
    /// </summary>
    Task<T> UpdatePartialAsync(T entity, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] properties);

    /// <summary>
    /// Toplu güncellemeler için kullanılır
    /// </summary>
    Task BulkUpdateAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default, params (Expression<Func<T, object>> Property, object Value)[] updates);

    /// <summary>
    /// Birden fazla varlığı günceller.
    /// </summary>
    Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bir varlığı siler. permanent false ise soft delete, true ise hard delete uygulanır.
    /// </summary>
    Task<T> DeleteAsync(T entity, bool permanent = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// soft delete kaldırma (yani geri alma) işlemi de yapılmak istenirse, aynı metodu softDelete parametresini false olarak çağırabilirsiniz:
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<T> RevertSoftDeleteAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Koşula göre bir varlığı siler.
    /// </summary>
    Task<T> DeleteAsync(Expression<Func<T, bool>> predicate, bool permanent = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Id Sütununa göre silme işlemi
    /// </summary>
    Task<T> DeleteAsync(TId id, bool permanent = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// soft delete kaldırma (yani geri alma) işlemi de yapılmak istenirse, aynı metodu softDelete parametresini false olarak çağırabilirsiniz:
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<T> RevertSoftDeleteAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Birden fazla varlığı siler.
    /// </summary>
    Task DeleteRangeAsync(IEnumerable<T> entities, bool permanent = false, CancellationToken cancellationToken = default);


}