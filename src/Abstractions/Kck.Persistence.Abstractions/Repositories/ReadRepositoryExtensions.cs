using System.Linq.Expressions;
using Kck.Core.Abstractions.Entities;
using Kck.Core.Abstractions.Paging;
using Kck.Persistence.Abstractions.Dynamic;

namespace Kck.Persistence.Abstractions.Repositories;

/// <summary>
/// Convenience extensions that accept <see cref="QueryOptions"/> instead of raw bool parameters.
/// </summary>
public static class ReadRepositoryExtensions
{
    /// <summary><see cref="QueryOptions"/>-based overload of <c>IReadRepository.GetAsync</c>.</summary>
    public static Task<T?> GetAsync<T, TId>(
        this IReadRepository<T, TId> repository,
        Expression<Func<T, bool>> predicate,
        QueryOptions options,
        Expression<Func<T, object>>[]? includes = null,
        CancellationToken cancellationToken = default)
        where T : Entity<TId>
    {
#pragma warning disable KCK0100
        return repository.GetAsync(predicate, includes, options.IncludeDeleted, options.AsTracking, cancellationToken);
#pragma warning restore KCK0100
    }

    /// <summary><see cref="QueryOptions"/>-based overload of <c>IReadRepository.GetListAsync</c>.</summary>
    public static Task<IPaginate<T>> GetListAsync<T, TId>( // NOSONAR S107 - deprecated API retained for backwards compatibility
        this IReadRepository<T, TId> repository,
        QueryOptions options,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Expression<Func<T, object>>[]? includes = null,
        int index = 0,
        int size = 10,
        CancellationToken cancellationToken = default)
        where T : Entity<TId>
    {
#pragma warning disable KCK0100
        return repository.GetListAsync(predicate, orderBy, includes, index, size, options.IncludeDeleted, options.AsTracking, cancellationToken);
#pragma warning restore KCK0100
    }

    /// <summary><see cref="QueryOptions"/>-based overload of <c>IReadRepository.GetListByDynamicAsync</c>.</summary>
    public static Task<IPaginate<T>> GetListByDynamicAsync<T, TId>( // NOSONAR S107 - deprecated API retained for backwards compatibility
        this IReadRepository<T, TId> repository,
        DynamicQuery dynamic,
        QueryOptions options,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>[]? includes = null,
        int index = 0,
        int size = 10,
        CancellationToken cancellationToken = default)
        where T : Entity<TId>
    {
#pragma warning disable KCK0100
        return repository.GetListByDynamicAsync(dynamic, predicate, includes, index, size, options.IncludeDeleted, options.AsTracking, cancellationToken);
#pragma warning restore KCK0100
    }

    /// <summary><see cref="QueryOptions"/>-based overload of <c>IReadRepository.AnyAsync</c>.</summary>
    public static Task<bool> AnyAsync<T, TId>(
        this IReadRepository<T, TId> repository,
        QueryOptions options,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
        where T : Entity<TId>
    {
#pragma warning disable KCK0100
        return repository.AnyAsync(predicate, options.IncludeDeleted, options.AsTracking, cancellationToken);
#pragma warning restore KCK0100
    }

    /// <summary><see cref="QueryOptions"/>-based overload of <c>IReadRepository.GetByIdAsync</c>.</summary>
    public static Task<T?> GetByIdAsync<T, TId>(
        this IReadRepository<T, TId> repository,
        TId id,
        QueryOptions options,
        CancellationToken cancellationToken = default)
        where T : Entity<TId>
    {
#pragma warning disable KCK0100
        return repository.GetByIdAsync(id, options.IncludeDeleted, options.AsTracking, cancellationToken);
#pragma warning restore KCK0100
    }

    /// <summary><see cref="QueryOptions"/>-based overload of <c>IReadRepository.CountAsync</c>.</summary>
    public static Task<int> CountAsync<T, TId>(
        this IReadRepository<T, TId> repository,
        QueryOptions options,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
        where T : Entity<TId>
    {
#pragma warning disable KCK0100
        return repository.CountAsync(predicate, options.IncludeDeleted, options.AsTracking, cancellationToken);
#pragma warning restore KCK0100
    }
}
