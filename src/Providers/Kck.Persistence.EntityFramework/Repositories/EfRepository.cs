using System.Linq.Expressions;
using Kck.Core.Abstractions.Entities;
using Kck.Core.Abstractions.Paging;
using Kck.Persistence.Abstractions.Dynamic;
using Kck.Persistence.Abstractions.Repositories;
using Kck.Persistence.Abstractions.Security;
using Kck.Persistence.EntityFramework.Extensions;
using Kck.Persistence.EntityFramework.Interceptors;
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
    // Deletes and reverts find the row whether or not it is already soft-deleted.
    private static readonly QueryOptions TrackingWithDeleted = new(IncludeDeleted: true, AsTracking: true);

    // The soft-delete filter applies to soft-deletable types only: for any other T the cast cannot be translated to
    // SQL (K1, 2026-10-31: every read of a plain Entity<TId> threw), and IncludeDeleted has nothing to lift.
    private static readonly bool IsSoftDeletable = typeof(ISoftDeletable).IsAssignableFrom(typeof(T));

    protected DbContext Context { get; } = context;
    protected DbSet<T> DbSet => Context.Set<T>();

    /// <inheritdoc />
    public IQueryable<T> Query(
        QueryOptions options,
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        IQueryable<T> query = DbSet;

        if (!options.AsTracking)
            query = query.AsNoTracking();

        // The repository's own filter, plus the model's named soft-delete filter when the context applies
        // ApplyGlobalSoftDeleteQueryFilter. Only that named filter is lifted: other filters (tenant...) stay.
        if (IsSoftDeletable)
        {
            query = options.IncludeDeleted
                ? query.IgnoreQueryFilters([ModelBuilderExtensions.SoftDeleteQueryFilterName])
                : query.Where(e => !((ISoftDeletable)e).IsDeleted);
        }

        if (filter is not null)
            query = query.Where(filter);

        if (orderBy is not null)
            query = orderBy(query);

        return query;
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync(
        Expression<Func<T, bool>> predicate,
        QueryOptions? options = null,
        Expression<Func<T, object>>[]? includes = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = Query(options ?? QueryOptions.Tracking, predicate);
        query = ApplyIncludes(query, includes);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IPaginate<T>> GetListAsync(
        QueryOptions? options = null,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Expression<Func<T, object>>[]? includes = null,
        int index = 0, int size = 10,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = Query(options ?? QueryOptions.Tracking, predicate, orderBy);
        query = ApplyIncludes(query, includes);
        return await query.ToPaginateAsync(index, size, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IPaginate<T>> GetListByDynamicAsync(
        DynamicQuery dynamic,
        QueryOptions? options = null,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>[]? includes = null,
        int index = 0, int size = 10,
        CancellationToken cancellationToken = default)
    {
        if (whitelist is null)
            throw new InvalidOperationException(
                $"Dynamic queries require a filter property whitelist for type '{typeof(T).Name}'. " +
                "Register an IFilterPropertyWhitelist<T> implementation or avoid using dynamic queries.");

        DynamicFilterWhitelistGuard.Validate(dynamic, whitelist);

        IQueryable<T> query = Query(options ?? QueryOptions.Tracking, predicate);
        query = ApplyIncludes(query, includes);
        query = query.ToDynamic(dynamic);
        return await query.ToPaginateAsync(index, size, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> AnyAsync(
        QueryOptions? options = null,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = Query(options ?? QueryOptions.None);
        return predicate is not null
            ? await query.AnyAsync(predicate, cancellationToken)
            : await query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(
        TId id,
        QueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = Query(options ?? QueryOptions.None);
        return await query.FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(
        QueryOptions? options = null,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = Query(options ?? QueryOptions.None);
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
    /// <exception cref="ArgumentException">A value does not match its property's CLR type exactly.</exception>
    public async Task BulkUpdateAsync(
        Expression<Func<T, bool>> predicate,
        (Expression<Func<T, object>> Property, object? Value)[] updates,
        CancellationToken cancellationToken = default)
    {
        // The repository's own rows: soft-deleted ones are left out, whether or not the model has the global filter.
        await BulkOperationHelper.BulkUpdateAsync<T, TId>(Query(QueryOptions.None, predicate), updates, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        DbSet.UpdateRange(entities);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<T> DeleteAsync(T entity, bool permanent = false, CancellationToken cancellationToken = default) // NOSONAR S4136 - interleaved with RevertSoftDeleteAsync; reordering shifts integration-tested overloads into new-code coverage scope
    {
        if (permanent)
        {
            EnsureAttached(entity);
            PermanentDeletes.Mark(Context, entity);
            DbSet.Remove(entity);
            return Task.FromResult(entity);
        }

        if (entity is ISoftDeletable)
        {
            SoftDeleteHelper.SoftDelete(Context, [entity], DateTime.UtcNow);
        }
        else
        {
            EnsureAttached(entity);
            DbSet.Remove(entity);
        }

        return Task.FromResult(entity);
    }

    /// <inheritdoc />
    public Task<T> RevertSoftDeleteAsync(T entity, CancellationToken cancellationToken = default) // NOSONAR S4136
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
        var entity = await Query(TrackingWithDeleted, predicate)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
            throw new InvalidOperationException(
                $"No {typeof(T).Name} matches the predicate (soft-deleted rows included), so nothing was deleted. " +
                "Check with GetAsync(predicate, QueryOptions.WithDeleted) first, or treat this as not found (for example a 404).");

        return await DeleteAsync(entity, permanent, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<T> DeleteAsync(
        TId id,
        bool permanent = false,
        CancellationToken cancellationToken = default)
    {
        var entity = await Query(TrackingWithDeleted)
            .FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken);

        if (entity is null)
            throw new InvalidOperationException(
                $"{typeof(T).Name} with id '{id}' not found (soft-deleted rows included), so nothing was deleted. " +
                "Check with GetByIdAsync(id, QueryOptions.WithDeleted) first, or treat this as not found (for example a 404).");

        return await DeleteAsync(entity, permanent, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<T> RevertSoftDeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await Query(TrackingWithDeleted)
            .FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken);

        if (entity is null)
            throw new InvalidOperationException(
                $"{typeof(T).Name} with id '{id}' not found, so there is nothing to restore. " +
                "Check with GetByIdAsync(id, QueryOptions.WithDeleted) first.");

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
            {
                EnsureAttached(entity);
                PermanentDeletes.Mark(Context, entity);
            }
            DbSet.RemoveRange(entityList);
            return Task.CompletedTask;
        }

        foreach (var entity in entityList.Where(e => e is not ISoftDeletable))
        {
            EnsureAttached(entity);
            DbSet.Remove(entity);
        }

        SoftDeleteHelper.SoftDelete(Context, entityList.OfType<ISoftDeletable>(), DateTime.UtcNow);
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
