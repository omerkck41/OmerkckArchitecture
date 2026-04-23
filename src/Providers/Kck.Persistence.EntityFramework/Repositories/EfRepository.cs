using System.Linq.Expressions;
using Kck.Core.Abstractions.Entities;
using Kck.Core.Abstractions.Paging;
using Kck.Persistence.Abstractions.Dynamic;
using Kck.Persistence.Abstractions.Repositories;
using Kck.Persistence.Abstractions.Security;
using Kck.Persistence.EntityFramework.Paging;
using Kck.Persistence.EntityFramework.Security;
using Microsoft.EntityFrameworkCore;

namespace Kck.Persistence.EntityFramework.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IRepository{T, TId}"/>.
/// </summary>
public class EfRepository<T, TId>(
    DbContext context,
    IFilterPropertyWhitelist<T>? whitelist = null) : IRepository<T, TId>
    where T : Entity<TId>
{
    protected DbContext Context { get; } = context;
    protected DbSet<T> DbSet => Context.Set<T>();

    /// <inheritdoc />
    public IQueryable<T> Query(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        bool withDeleted = false,
        bool enableTracking = false)
    {
        IQueryable<T> query = DbSet;

        if (!enableTracking)
            query = query.AsNoTracking();

        if (!withDeleted)
            query = query.Where(e => !((ISoftDeletable)e).IsDeleted);

        if (filter is not null)
            query = query.Where(filter);

        if (orderBy is not null)
            query = orderBy(query);

        return query;
    }

    /// <summary>
    /// Convenience overload that accepts a <see cref="QueryOptions"/> value object
    /// instead of positional boolean flags.
    /// </summary>
    public IQueryable<T> Query(
        QueryOptions options,
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        => Query(filter, orderBy, options.IncludeDeleted, options.AsTracking);

    /// <inheritdoc />
    public async Task<T?> GetAsync(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, object>>[]? includes = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = Query(predicate, withDeleted: withDeleted, enableTracking: enableTracking);
        query = ApplyIncludes(query, includes);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IPaginate<T>> GetListAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Expression<Func<T, object>>[]? includes = null,
        int index = 0, int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = Query(predicate, orderBy, withDeleted, enableTracking);
        query = ApplyIncludes(query, includes);
        return await query.ToPaginateAsync(index, size, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IPaginate<T>> GetListByDynamicAsync(
        DynamicQuery dynamic,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>[]? includes = null,
        int index = 0, int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        if (whitelist is null)
            throw new InvalidOperationException(
                $"Dynamic queries require a filter property whitelist for type '{typeof(T).Name}'. " +
                "Register an IFilterPropertyWhitelist<T> implementation or avoid using dynamic queries.");

        DynamicFilterWhitelistGuard.Validate(dynamic, whitelist);

        IQueryable<T> query = Query(predicate, withDeleted: withDeleted, enableTracking: enableTracking);
        query = ApplyIncludes(query, includes);
        query = query.ToDynamic(dynamic);
        return await query.ToPaginateAsync(index, size, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> AnyAsync(
        Expression<Func<T, bool>>? predicate = null,
        bool withDeleted = false,
        bool enableTracking = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = Query(withDeleted: withDeleted, enableTracking: enableTracking);
        return predicate is not null
            ? await query.AnyAsync(predicate, cancellationToken)
            : await query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(
        TId id,
        bool withDeleted = false,
        bool enableTracking = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = Query(withDeleted: withDeleted, enableTracking: enableTracking);
        return await query.FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        bool withDeleted = false,
        bool enableTracking = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = Query(withDeleted: withDeleted, enableTracking: enableTracking);
        return predicate is not null
            ? await query.CountAsync(predicate, cancellationToken)
            : await query.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<T>> AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default)
    {
        var entityList = entities as IList<T> ?? entities.ToList();
        await DbSet.AddRangeAsync(entityList, cancellationToken);
        return entityList;
    }

    /// <inheritdoc />
    public Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        Context.Attach(entity);
        Context.Entry(entity).State = EntityState.Modified;
        return Task.FromResult(entity);
    }

    /// <inheritdoc />
    public Task<T> UpdatePartialAsync(
        T entity,
        Expression<Func<T, object>>[] properties,
        CancellationToken cancellationToken = default)
    {
        var entry = Context.Attach(entity);
        entry.State = EntityState.Unchanged;

        foreach (var property in properties)
            entry.Property(property).IsModified = true;

        return Task.FromResult(entity);
    }

    /// <summary>
    /// Performs a bulk update using EF Core's ExecuteUpdateAsync (single SQL statement).
    /// <para>
    /// WARNING: Bypasses change tracker and interceptors (e.g., audit).
    /// For audited updates, use <see cref="UpdateAsync"/> or <see cref="UpdatePartialAsync"/> instead.
    /// </para>
    /// </summary>
    /// <inheritdoc />
    public async Task BulkUpdateAsync(
        Expression<Func<T, bool>> predicate,
        (Expression<Func<T, object>> Property, object? Value)[] updates,
        CancellationToken cancellationToken = default)
    {
        await BulkOperationHelper.BulkUpdateAsync<T, TId>(DbSet, predicate, updates, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        DbSet.UpdateRange(entities);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<T> DeleteAsync(T entity, bool permanent = false, CancellationToken cancellationToken = default)
    {
        if (permanent)
        {
            EnsureAttached(entity);
            DbSet.Remove(entity);
            return Task.FromResult(entity);
        }

        if (entity is ISoftDeletable softDeletable)
        {
            softDeletable.IsDeleted = true;
            softDeletable.DeletedDate = DateTime.UtcNow;
            Context.Entry(entity).State = EntityState.Modified;

            SoftDeleteHelper.CascadeSoftDelete(Context, entity);
        }
        else
        {
            EnsureAttached(entity);
            DbSet.Remove(entity);
        }

        return Task.FromResult(entity);
    }

    /// <inheritdoc />
    public Task<T> RevertSoftDeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity is ISoftDeletable softDeletable)
        {
            softDeletable.IsDeleted = false;
            softDeletable.DeletedBy = null;
            softDeletable.DeletedDate = null;
            Context.Entry(entity).State = EntityState.Modified;
        }

        return Task.FromResult(entity);
    }

    /// <inheritdoc />
    public async Task<T> DeleteAsync(
        Expression<Func<T, bool>> predicate,
        bool permanent = false,
        CancellationToken cancellationToken = default)
    {
        var entity = await Query(predicate, withDeleted: true, enableTracking: true)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
            throw new InvalidOperationException("Entity not found for deletion.");

        return await DeleteAsync(entity, permanent, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<T> DeleteAsync(
        TId id,
        bool permanent = false,
        CancellationToken cancellationToken = default)
    {
        var entity = await Query(withDeleted: true, enableTracking: true)
            .FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken);

        if (entity is null)
            throw new InvalidOperationException($"Entity with id '{id}' not found for deletion.");

        return await DeleteAsync(entity, permanent, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<T> RevertSoftDeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await Query(withDeleted: true, enableTracking: true)
            .FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken);

        if (entity is null)
            throw new InvalidOperationException($"Entity with id '{id}' not found for soft-delete revert.");

        return await RevertSoftDeleteAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteRangeAsync(
        IEnumerable<T> entities,
        bool permanent = false,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var entityList = entities as IList<T> ?? entities.ToList();
        if (entityList.Count == 0)
            return Task.CompletedTask;

        if (permanent)
        {
            foreach (var entity in entityList)
                EnsureAttached(entity);
            DbSet.RemoveRange(entityList);
            return Task.CompletedTask;
        }

        var now = DateTime.UtcNow;
        foreach (var entity in entityList)
        {
            if (entity is ISoftDeletable softDeletable)
            {
                softDeletable.IsDeleted = true;
                softDeletable.DeletedDate = now;
                Context.Entry(entity).State = EntityState.Modified;
                SoftDeleteHelper.CascadeSoftDelete(Context, entity);
            }
            else
            {
                EnsureAttached(entity);
                DbSet.Remove(entity);
            }
        }

        return Task.CompletedTask;
    }

    private static IQueryable<T> ApplyIncludes(IQueryable<T> query, Expression<Func<T, object>>[]? includes)
    {
        if (includes is null)
            return query;

        foreach (var include in includes)
            query = query.Include(include);

        return query;
    }

    private void EnsureAttached(T entity)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
            Context.Attach(entity);
    }

}
