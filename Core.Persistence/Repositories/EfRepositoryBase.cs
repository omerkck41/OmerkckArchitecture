using Core.Persistence.Dynamic;
using Core.Persistence.Entities;
using Core.Persistence.Paging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Core.Persistence.Repositories;

public class EfRepositoryBase<TEntity, TContext> : IAsyncRepository<TEntity> where TEntity : Entity where TContext : DbContext
{
    protected readonly TContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public EfRepositoryBase(TContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }

    public IQueryable<TEntity> Query()
    {
        return _dbSet.AsQueryable().Where(e => !e.IsDeleted);
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
                                                                Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>[]? includes = null,
                                                                int index = 0, int size = 10,
                                                                bool enableTracking = true,
                                                                CancellationToken cancellationToken = default)
    {
        // Başlangıç sorgusu: Tracking durumu kontrol ediliyor
        IQueryable<TEntity> query = enableTracking ? Query() : Query().AsNoTracking();

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


    public async Task<TEntity?> GetByIdAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        return entity switch
        {
            null => throw new KeyNotFoundException($"{typeof(TEntity).Name} with id {id} was not found."),
            _ => entity,
        };
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
    {
        return predicate == null
            ? await _dbSet.CountAsync()
            : await _dbSet.CountAsync(predicate);
    }

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        var entry = await _dbSet.AddAsync(entity);
        return entry.Entity;
    }

    public Task UpdateAsync(TEntity entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task<TEntity> UpdatePartialAsync(TEntity entity, params Expression<Func<TEntity, object>>[] properties)
    {
        // Entity'yi takip altına al
        //_dbSet.Attach(entity);

        // Belirtilen alanları güncelle
        foreach (var property in properties)
        {
            var propertyName = property.GetPropertyAccess().Name;
            _context.Entry(entity).Property(propertyName).IsModified = true;
        }

        // UnitOfWork tarafından SaveChanges çağrılacak, burada yalnızca Entity döndürülür
        return Task.FromResult(entity);
    }

    public async Task BulkUpdateAsync(Expression<Func<TEntity, bool>> predicate, params (Expression<Func<TEntity, object>> Property, object Value)[] updates)
    {
        var entities = await _dbSet.Where(predicate).ToListAsync();

        foreach (var entity in entities)
        {
            _dbSet.Attach(entity);

            foreach (var (Property, Value) in updates)
            {
                var propertyName = Property.GetPropertyAccess().Name;
                _context.Entry(entity).Property(propertyName).CurrentValue = Value;
                _context.Entry(entity).Property(propertyName).IsModified = true;
            }
        }
    }

    public Task<TEntity> DeleteAsync(TEntity entity)
    {
        _dbSet.Remove(entity);
        return Task.FromResult(entity);
    }
    public async Task<TEntity> DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id) ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} with id {id} was not found.");

        _dbSet.Remove(entity);
        return await Task.FromResult(entity);
    }


    public async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        return entities;
    }

    public Task UpdateRangeAsync(IEnumerable<TEntity> entities)
    {
        _dbSet.UpdateRange(entities);
        return Task.CompletedTask;
    }

    public Task DeleteRangeAsync(IEnumerable<TEntity> entities)
    {
        _dbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }


    public Task<TEntity> SoftDeleteAsync(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity), $"{typeof(TEntity).Name} entity cannot be null.");

        entity.IsDeleted = true; // Soft Delete işlemi
        entity.DeletedDate = DateTime.UtcNow;
        _dbSet.Update(entity); // Entity'nin değişiklikleri takip ediliyor

        return Task.FromResult(entity); // Task döndürmek için CompletedTask kullanılır
    }
    public Task<TEntity> SoftDeleteAsync(TEntity entity, string deletedBy)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity), $"{typeof(TEntity).Name} entity cannot be null.");

        entity.IsDeleted = true;
        entity.DeletedDate = DateTime.UtcNow;
        entity.DeletedBy = deletedBy; // Silen kullanıcının bilgisi eklenebilir
        _dbSet.Update(entity);

        return Task.FromResult(entity);
    }

    public async Task<TEntity> SoftDeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id)
                 ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} with id {id} was not found.");

        entity.IsDeleted = true;
        entity.DeletedDate = DateTime.UtcNow;

        _dbSet.Update(entity); // Değişiklik izleme
        return await Task.FromResult(entity); // Geri dönüş olarak güncellenmiş entity
    }
    public async Task<TEntity> SoftDeleteAsync(int id, string deletedBy)
    {
        var entity = await _dbSet.FindAsync(id)
                 ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} with id {id} was not found.");

        entity.IsDeleted = true;
        entity.DeletedDate = DateTime.UtcNow;
        entity.DeletedBy = deletedBy; // Silen kullanıcı bilgisi eklenebilir
        _dbSet.Update(entity);

        return await Task.FromResult(entity);
    }

    public Task SoftDeleteRangeAsync(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            entity.IsDeleted = true; // Soft Delete işlemi
            entity.DeletedDate = DateTime.UtcNow;
        }
        _dbSet.UpdateRange(entities); // Toplu güncelleme
        return Task.CompletedTask;
    }
    public Task SoftDeleteRangeAsync(IEnumerable<TEntity> entities, string deletedBy)
    {
        foreach (var entity in entities)
        {
            entity.IsDeleted = true;
            entity.DeletedDate = DateTime.UtcNow;
            entity.DeletedBy = deletedBy;
        }
        _dbSet.UpdateRange(entities);
        return Task.CompletedTask;
    }


}