using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Core.Persistence.Dynamic;
using Core.Persistence.Entities;
using Core.Persistence.Paging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
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

    public IQueryable<TEntity> Query(Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool withDeleted = false,
        bool enableTracking = false)
    {
        IQueryable<TEntity> query = enableTracking ? _dbSet : _dbSet.AsNoTracking();

        if (!withDeleted && typeof(TEntity).GetProperty("IsDeleted") != null)
            query = query.Where(e => EF.Property<bool>(e, "IsDeleted") == false);

        if (filter != null)
            query = query.Where(filter);

        if (orderBy != null)
            query = orderBy(query);

        return query;
    }



    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate,
                                            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>[]? includes = null,
                                            bool withDeleted = false,
                                            bool enableTracking = true,
                                            CancellationToken cancellationToken = default)
    {
        var query = Query(withDeleted: withDeleted, enableTracking: enableTracking);

        if (includes != null)
            foreach (var include in includes)
                query = include(query);

        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IPaginate<TEntity>> GetListAsync(Expression<Func<TEntity, bool>>? predicate = null,
                                                         Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
                                                         Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>[]? includes = null,
                                                         int index = 0, int size = 10,
                                                         bool withDeleted = false,
                                                         bool enableTracking = true,
                                                         CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = Query(withDeleted: withDeleted, enableTracking: enableTracking);

        if (predicate != null)
            query = query.Where(predicate);

        if (includes != null)
            foreach (var include in includes)
                query = include(query);

        var paginatedQuery = orderBy != null ? orderBy(query) : query;

        return await paginatedQuery.ToPaginateAsync(index, size, 0, cancellationToken);
    }

    public async Task<IPaginate<TEntity>> GetListByDynamicAsync(Dynamic.Dynamic dynamic,
                                                                Expression<Func<TEntity, bool>>? predicate = null,
                                                                Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>[]? includes = null,
                                                                int index = 0, int size = 10,
                                                                bool withDeleted = false,
                                                                bool enableTracking = true,
                                                                CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = Query(withDeleted: withDeleted, enableTracking: enableTracking);

        if (predicate != null)
            query = query.Where(predicate);

        query = query.ToDynamic(dynamic);

        if (includes != null)
            foreach (var include in includes)
                query = include(query);

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
                                                                       bool withDeleted = false,
                                                                       bool enableTracking = true,
                                                                       CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = Query(withDeleted: withDeleted, enableTracking: enableTracking);

        if (include != null)
            query = include(query);

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
                                    bool withDeleted = false,
                                    bool enableTracking = false,
                                    CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = Query(withDeleted: withDeleted, enableTracking: enableTracking);

        if (include != null)
            query = include(query);

        if (predicate != null)
            query = query.Where(predicate);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<TEntity?> GetByIdAsync(TId id, bool withDeleted = false, bool enableTracking = false, CancellationToken cancellationToken = default)
    {
        if (id == null)
            throw new CustomArgumentException(nameof(id), "Id cannot be null.");

        var entity = await Query(withDeleted: withDeleted, enableTracking: enableTracking)
                            .FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken);

        return entity ?? throw new NotFoundException($"{typeof(TEntity).Name} with id {id} was not found.");
    }


    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, bool withDeleted = false, bool enableTracking = false, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = Query(withDeleted: withDeleted, enableTracking: enableTracking);

        if (predicate != null)
            query = query.Where(predicate);

        return await query.CountAsync(cancellationToken);
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

    private void ApplyUpdates(TEntity entity, params (Expression<Func<TEntity, object>> Property, object? Value)[] updates)
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
    public async Task BulkUpdateAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params (Expression<Func<TEntity, object>> Property, object? Value)[] updates)
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
            throw new CustomArgumentException(nameof(entity), $"{typeof(TEntity).Name} cannot be null.");

        if (!permanent && typeof(TEntity).GetProperty("IsDeleted") != null)
        {
            // Soft delete işlemi
            entity.IsDeleted = true;
            entity.DeletedDate = DateTime.UtcNow;
            entity.DeletedBy ??= "OmerkckArchitecture";

            // Cascade soft delete: Bağlı entity’lerde de soft delete uygulaması
            await CascadeSoftDeleteAsync(entity, true, cancellationToken: cancellationToken);

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
            throw new CustomArgumentException(nameof(entity), $"{typeof(TEntity).Name} cannot be null.");

        if (typeof(TEntity).GetProperty("IsDeleted") == null)
            throw new CustomInvalidOperationException("Entity does not support soft delete.");

        // Soft delete kaldırma işlemi
        entity.IsDeleted = false;
        entity.DeletedDate = null;
        entity.DeletedBy = null;

        // Cascade revert soft delete: Bağlı entity'lerde de soft delete kaldırma
        await CascadeSoftDeleteAsync(entity, false, cancellationToken: cancellationToken);
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

        return entity == null
            ? throw new NotFoundException($"{typeof(TEntity).Name} satisfying the condition was not found.")
            : await DeleteAsync(entity, permanent, cancellationToken);
    }

    /// <summary>
    /// Belirtilen Id'ye göre varlığı siler.
    /// </summary>
    public async Task<TEntity> DeleteAsync(TId id, bool permanent = false, CancellationToken cancellationToken = default)
    {
        // FindAsync optimizasyon sağladığı için tercih ediliyor.
        var entity = await _dbSet.FindAsync([id], cancellationToken)
            ?? throw new NotFoundException($"{typeof(TEntity).Name} with id {id} was not found.");

        if (!permanent && typeof(TEntity).GetProperty("IsDeleted") != null)
        {
            // Entity'yi soft delete olarak işaretle
            entity.IsDeleted = true;
            entity.DeletedDate = DateTime.UtcNow;
            entity.DeletedBy ??= "OmerkckArchitecture";

            // Cascade soft delete: bağlı entity'lerde de soft delete işlemi uygula
            await CascadeSoftDeleteAsync(entity, true, cancellationToken: cancellationToken);
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
        var entity = await _dbSet.FindAsync([id], cancellationToken)
            ?? throw new NotFoundException($"{typeof(TEntity).Name} with id {id} was not found.");

        if (typeof(TEntity).GetProperty("IsDeleted") != null)
        {
            // Soft delete kaldırılıyor
            entity.IsDeleted = false;
            entity.DeletedDate = null;
            entity.DeletedBy = null;

            await CascadeSoftDeleteAsync(entity, false, cancellationToken: cancellationToken);
            _dbSet.Update(entity);
        }
        else
        {
            throw new CustomArgumentException("Entity does not support soft delete.");
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
                entity.DeletedBy ??= "OmerkckArchitecture";
                await CascadeSoftDeleteAsync(entity, true, cancellationToken: cancellationToken);
            }
            _dbSet.UpdateRange(entities);
        }
        else
        {
            _dbSet.RemoveRange(entities);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }



    /// <summary>
    /// Cascade soft delete veya geri alma işlemini uygulayan yardımcı metot.
    /// </summary>
    private async Task CascadeSoftDeleteAsync(object entity, bool softDelete, HashSet<object>? visited = null, CancellationToken cancellationToken = default)
    {
        visited ??= new HashSet<object>();
        if (visited.Contains(entity))
            return;
        visited.Add(entity);

        var entry = _context.Entry(entity);
        foreach (var navigation in entry.Navigations)
        {
            // Eğer navigation property, dependent (yani parent'a işaret eden) ise cascade işlemine dahil edilmesin.
            if (navigation.Metadata is INavigation navMetadata && navMetadata.IsOnDependent)
                continue;

            if (!navigation.IsLoaded)
                await navigation.LoadAsync(cancellationToken);

            var related = navigation.CurrentValue;
            if (related == null)
                continue;

            if (related is IEnumerable<object> collection)
            {
                foreach (var item in collection)
                {
                    if (item == null)
                        continue;
                    await SoftDeleteAndCascadeAsync(item, softDelete, visited, cancellationToken);
                }
            }
            else
            {
                await SoftDeleteAndCascadeAsync(related, softDelete, visited, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Belirtilen entity üzerinde soft delete ayarlarını uygulayıp, cascade işlemini başlatır.
    /// </summary>
    private async Task SoftDeleteAndCascadeAsync(object entity, bool softDelete, HashSet<object> visited, CancellationToken cancellationToken)
    {
        if (entity is IAuditable auditable)
        {
            auditable.IsDeleted = softDelete;
            auditable.DeletedDate = softDelete ? DateTime.UtcNow : null;
            auditable.DeletedBy = softDelete ? "OmerkckArchitecture" : null;

            _context.Update((object)auditable);
        }
        else
        {
            // Eğer entity IAuditable değilse, ReflectionDelegateCache kullanımı devam edebilir.
            var (GetIsDeleted, SetIsDeleted, GetDeletedDate, SetDeletedDate, GetDeletedBy, SetDeletedBy) =
                ReflectionDelegateCache.GetDelegates(entity.GetType());

            if (SetIsDeleted != null)
            {
                SetIsDeleted(entity, softDelete);
                SetDeletedDate?.Invoke(entity, softDelete ? DateTime.UtcNow : null);
                SetDeletedBy?.Invoke(entity, softDelete ? "OmerkckArchitecture" : null);
                _context.Update(entity);
            }
        }
        await CascadeSoftDeleteAsync(entity, softDelete, visited, cancellationToken);
    }
}