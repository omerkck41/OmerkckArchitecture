using Kck.Persistence.Abstractions.Repositories;
using Kck.Persistence.Abstractions.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Kck.Persistence.EntityFramework.Specifications;

/// <summary>
/// Runs an <see cref="ISpecification{T}"/> against a repository. The repository's query applies the soft-delete rule
/// (<see cref="ISpecification{T}.WithDeleted"/>) and tracking (<see cref="ISpecification{T}.EnableTracking"/>); the
/// specification adds its criteria, includes, order and paging.
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>The repository's query with the whole specification applied.</summary>
    public static IQueryable<T> WithSpecification<T>(this IQuery<T> repository, ISpecification<T> specification)
        where T : class
    {
        var query = Filtered(repository, specification);
        query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));
        query = specification.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        if (specification.OrderBy is not null)
            query = query.OrderBy(specification.OrderBy);
        else if (specification.OrderByDescending is not null)
            query = query.OrderByDescending(specification.OrderByDescending);

        if (specification.Skip is { } skip)
            query = query.Skip(skip);
        if (specification.Take is { } take)
            query = query.Take(take);
        return query;
    }

    /// <summary>The entities the specification selects.</summary>
    public static Task<List<T>> ListAsync<T>(
        this IQuery<T> repository, ISpecification<T> specification, CancellationToken cancellationToken = default)
        where T : class =>
        repository.WithSpecification(specification).ToListAsync(cancellationToken);

    /// <summary>The first entity the specification selects, or <c>null</c>.</summary>
    public static Task<T?> FirstOrDefaultAsync<T>(
        this IQuery<T> repository, ISpecification<T> specification, CancellationToken cancellationToken = default)
        where T : class =>
        repository.WithSpecification(specification).FirstOrDefaultAsync(cancellationToken);

    /// <summary>The number of entities matching the criteria, ignoring paging (a total for a paged list).</summary>
    public static Task<int> CountAsync<T>(
        this IQuery<T> repository, ISpecification<T> specification, CancellationToken cancellationToken = default)
        where T : class =>
        Filtered(repository, specification).CountAsync(cancellationToken);

    private static IQueryable<T> Filtered<T>(IQuery<T> repository, ISpecification<T> specification)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(specification);
        return repository.Query(new QueryOptions(specification.WithDeleted, specification.EnableTracking), specification.Criteria);
    }
}
