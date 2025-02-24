using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Core.Persistence.Dynamic;
using Core.Persistence.Entities;
using Core.Persistence.Paging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Core.Persistence.Repositories;

public class EfRepositoryBase<TEntity, TId, TContext> : IAsyncRepository<TEntity, TId> where TEntity : Entity<TId> where TContext : DbContext
{
    protected readonly TContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public EfRepositoryBase(TContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }

    public IQueryable<TEntity> Query(Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null)
    {
        IQueryable<TEntity> query = _dbSet.AsQueryable();

        if (filter != null)
            query = query.Where(filter);

        if (orderBy != null)
            query = orderBy(query);

        return query;
    }



    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate,
                                            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>[]? includes = null,
                                            bool enableTracking = true,
                                            CancellationToken cancellationToken = default)
    {
        var query = enableTracking ? Query() : Query().AsNoTracking();

        if (includes != null)
            foreach (var include in includes)
                query = include(query);

        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IPaginate<TEntity>> GetListAsync(Expression<Func<TEntity, bool>>? predicate = null,
                                                         Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
                                                         Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>[]? includes = null,
                                                         int index = 0, int size = 10, bool enableTracking = true,
                                                         CancellationToken cancellationToken = default)
    {
        // Sorgu başlangıcı (tracking durumu kontrol ediliyor)
        IQueryable<TEntity> query = enableTracking ? Query() : Query().AsNoTracking();

        // Predicate varsa, sorguya ekleniyor
        if (predicate != null) query = query.Where(predicate);

        // Includes dizisi varsa, her bir include uygulanıyor
        if (includes != null)
            foreach (var include in includes)
                query = include(query);

        // OrderBy varsa sıralama yapılıyor, yoksa default olarak ToPaginateAsync çağrılıyor
        var paginatedQuery = orderBy != null
            ? orderBy(query)
            : query;

        // ToPaginateAsync kullanılarak sonuç dönüyor
        return await paginatedQuery.ToPaginateAsync(index, size, 0, cancellationToken);
    }

    public async Task<IPaginate<TEntity>> GetListByDynamicAsync(Dynamic.Dynamic dynamic,
                                                                Expression<Func<TEntity, bool>>? predicate = null,
                                                                Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>[]? includes = null,
                                                                int index = 0, int size = 10,
                                                                bool enableTracking = true,
                                                                CancellationToken cancellationToken = default)
    {
        // Başlangıç sorgusu: Tracking durumu kontrol ediliyor
        IQueryable<TEntity> query = enableTracking ? Query() : Query().AsNoTracking();

        // Predicate varsa, sorguya ekleniyor
        if (predicate != null) query = query.Where(predicate);

        // Dinamik sorgu uygulanıyor
        query = query.ToDynamic(dynamic);

        // Includes dizisi varsa, her bir include uygulanıyor
        if (includes != null)
            foreach (var include in includes)
                query = include(query);

        // ToPaginateAsync kullanılarak sayfalama yapılıyor
        return await query.ToPaginateAsync(index, size, 0, cancellationToken);
    }

    /// <summary>
    /// ex : var personels = await _context.Personels.Include(p => p.Department).ToListAsync();
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="include"></param>
    /// <param name="index"></param>
    /// <param name="size"></param>
    /// <param name="enableTracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IPaginate<TEntity>> GetListWithEagerLoadingAsync(Expression<Func<TEntity, bool>>? predicate = null,
                                                                       Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
                                                                       int index = 0,
                                                                       int size = 10,
                                                                       bool enableTracking = true,
                                                                       CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = enableTracking ? Query() : Query().AsNoTracking();

        if (include != null)
            query = include(query); // Eager Loading burada uygulanır

        if (predicate != null)
            query = query.Where(predicate);

        return await query.ToPaginateAsync(index, size, 0, cancellationToken);
    }

    /// <summary>
    /// Verilen koşula göre bir kaydın var olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="predicate">Kontrol edilecek koşul.</param>
    /// <param name="include">Eager loading için kullanılacak ilişkiler.</param>
    /// <param name="enableTracking">EF Core'un tracking davranışını kontrol eder.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Kayıt varsa true, yoksa false.</returns>
    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null,
                                    Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
                                    bool enableTracking = false,
                                    CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = enableTracking ? Query() : Query().IgnoreQueryFilters();

        if (include != null)
            query = include(query);

        if (predicate != null)
            query = query.Where(predicate);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.AsQueryable()
                                 .Where(e => e.Id != null && e.Id.Equals(id))
                                 .FirstOrDefaultAsync(cancellationToken);

        return entity switch
        {
            null => throw new CustomException($"{typeof(TEntity).Name} with id {id} was not found."),
            _ => entity,
        };
    }


    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
    {
        return predicate == null
            ? await _dbSet.CountAsync()
            : await _dbSet.CountAsync(predicate);
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var entry = await _dbSet.AddAsync(entity, cancellationToken);
        return entry.Entity;
    }
    public async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
        return entities;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }
    public async Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        _dbSet.UpdateRange(entities);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private void ApplyUpdates<TEntity>(TEntity entity, params (Expression<Func<TEntity, object>> Property, object Value)[] updates)
    {
        foreach (var (Property, Value) in updates)
        {
            var propertyName = Property.GetPropertyAccess().Name;
            _context.Entry(entity).Property(propertyName).CurrentValue = Value;
            _context.Entry(entity).Property(propertyName).IsModified = true;
        }
    }
    public async Task<TEntity> UpdatePartialAsync(TEntity entity, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] properties)
    {
        ApplyUpdates(entity, properties.Select(p => (p, _context.Entry(entity).Property(p.GetPropertyAccess().Name).CurrentValue)).ToArray());
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }
    public async Task BulkUpdateAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params (Expression<Func<TEntity, object>> Property, object Value)[] updates)
    {
        var entities = await _dbSet.Where(predicate).ToListAsync(cancellationToken);

        foreach (var entity in entities)
        {
            _dbSet.Attach(entity);
            ApplyUpdates(entity, updates);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }


    /// <summary>
    /// Tek bir varlığı siler. permanent false ise soft delete uygulanır; true ise hard delete yapılır.
    /// </summary>
    public async Task<TEntity> DeleteAsync(TEntity entity, bool permanent = false, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new CustomException(nameof(entity), $"{typeof(TEntity).Name} cannot be null.");

        if (!permanent && typeof(TEntity).GetProperty("IsDeleted") != null)
        {
            // Soft delete işlemi
            entity.IsDeleted = true;
            entity.DeletedDate = DateTime.UtcNow;
            entity.DeletedBy = entity.DeletedBy ?? "System";

            // Cascade soft delete: Bağlı entity’lerde de soft delete uygulaması
            await CascadeSoftDeleteAsync(entity, true, cancellationToken);

            _dbSet.Update(entity);
        }
        else
        {
            _dbSet.Remove(entity);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }
    public async Task<TEntity> RevertSoftDeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new CustomException(nameof(entity), $"{typeof(TEntity).Name} cannot be null.");

        if (typeof(TEntity).GetProperty("IsDeleted") == null)
            throw new CustomException("Entity does not support soft delete.");

        // Soft delete kaldırma işlemi
        entity.IsDeleted = false;
        entity.DeletedDate = null;
        entity.DeletedBy = null;

        // Cascade revert soft delete: Bağlı entity'lerde de soft delete kaldırma
        await CascadeSoftDeleteAsync(entity, false, cancellationToken);
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }
    /// <summary>
    /// Belirtilen koşula göre ilk bulunan varlığı siler.
    /// </summary>
    public async Task<TEntity> DeleteAsync(Expression<Func<TEntity, bool>> predicate, bool permanent = false, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        if (entity == null)
            throw new CustomException($"{typeof(TEntity).Name} satisfying the condition was not found.");

        return await DeleteAsync(entity, permanent, cancellationToken);
    }

    /// <summary>
    /// Belirtilen Id'ye göre varlığı siler.
    /// </summary>
    public async Task<TEntity> DeleteAsync(TId id, bool permanent = false, CancellationToken cancellationToken = default)
    {
        // FindAsync optimizasyon sağladığı için tercih ediliyor.
        var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        if (entity == null)
            throw new CustomException($"{typeof(TEntity).Name} with id {id} was not found.");

        if (!permanent && typeof(TEntity).GetProperty("IsDeleted") != null)
        {
            // Entity'yi soft delete olarak işaretle
            entity.IsDeleted = true;
            entity.DeletedDate = DateTime.UtcNow;
            entity.DeletedBy = entity.DeletedBy ?? "System";

            // Cascade soft delete: bağlı entity'lerde de soft delete işlemi uygula
            await CascadeSoftDeleteAsync(entity, true, cancellationToken);
            _dbSet.Update(entity);
        }
        else
        {
            _dbSet.Remove(entity);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }
    public async Task<TEntity> RevertSoftDeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        if (entity == null)
            throw new CustomException($"{typeof(TEntity).Name} with id {id} was not found.");

        if (typeof(TEntity).GetProperty("IsDeleted") != null)
        {
            // Soft delete kaldırılıyor
            entity.IsDeleted = false;
            entity.DeletedDate = null;
            entity.DeletedBy = null;

            await CascadeSoftDeleteAsync(entity, false, cancellationToken);
            _dbSet.Update(entity);
        }
        else
        {
            throw new CustomException("Entity does not support soft delete.");
        }

        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <summary>
    /// Birden fazla varlığı siler. permanent false ise soft delete, true ise hard delete yapılır.
    /// </summary>
    public async Task DeleteRangeAsync(IEnumerable<TEntity> entities, bool permanent = false, CancellationToken cancellationToken = default)
    {
        if (entities == null || !entities.Any())
            return;

        if (!permanent && typeof(TEntity).GetProperty("IsDeleted") != null)
        {
            foreach (var entity in entities)
            {
                entity.IsDeleted = true;
                entity.DeletedDate = DateTime.UtcNow;
                entity.DeletedBy = entity.DeletedBy ?? "System";
                await CascadeSoftDeleteAsync(entity, true, cancellationToken);
            }
            _dbSet.UpdateRange(entities);
        }
        else
        {
            _dbSet.RemoveRange(entities);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }



    // Cascade soft delete veya geri alma işlemini uygulayan yardımcı metot.
    private async Task CascadeSoftDeleteAsync(object entity, bool softDelete, CancellationToken cancellationToken = default, HashSet<object>? visited = null)
    {
        visited ??= new HashSet<object>();
        if (visited.Contains(entity))
            return;
        visited.Add(entity);

        var entry = _context.Entry(entity);
        foreach (var navigation in entry.Navigations)
        {
            if (!navigation.IsLoaded)
                await navigation.LoadAsync(cancellationToken);

            var related = navigation.CurrentValue;
            if (related == null)
                continue;

            if (related is IEnumerable<object> collection)
            {
                foreach (var item in collection)
                {
                    if (item == null) continue;

                    var delegates = ReflectionDelegateCache.GetDelegates(item.GetType());
                    if (delegates.SetIsDeleted != null)
                    {
                        delegates.SetIsDeleted(item, softDelete);
                        delegates.SetDeletedDate?.Invoke(item, softDelete ? DateTime.UtcNow : null);
                        delegates.SetDeletedBy?.Invoke(item, softDelete ? "System" : null);

                        _context.Update(item);
                        await CascadeSoftDeleteAsync(item, softDelete, cancellationToken, visited);
                    }
                }
            }
            else
            {
                var delegates = ReflectionDelegateCache.GetDelegates(related.GetType());
                if (delegates.SetIsDeleted != null)
                {
                    delegates.SetIsDeleted(related, softDelete);
                    delegates.SetDeletedDate?.Invoke(related, softDelete ? DateTime.UtcNow : null);
                    delegates.SetDeletedBy?.Invoke(related, softDelete ? "System" : null);

                    _context.Update(related);
                    await CascadeSoftDeleteAsync(related, softDelete, cancellationToken, visited);
                }
            }
        }
    }
}