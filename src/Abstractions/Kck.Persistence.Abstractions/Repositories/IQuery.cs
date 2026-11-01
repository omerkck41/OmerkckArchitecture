using System.Linq.Expressions;

namespace Kck.Persistence.Abstractions.Repositories;

/// <summary>
/// Low-level <see cref="IQueryable{T}"/> access for custom LINQ queries. Prefer <c>IReadRepository</c> where possible.
/// </summary>
public interface IQuery<T>
{
    /// <summary>
    /// Returns an <see cref="IQueryable{T}"/> filtered and sorted per the provided parameters.
    /// </summary>
    /// <param name="options">Soft-deleted rows and change tracking; <see cref="QueryOptions.None"/> excludes both.</param>
    /// <param name="filter">Optional predicate.</param>
    /// <param name="orderBy">Optional ordering.</param>
    IQueryable<T> Query(QueryOptions options,
                        Expression<Func<T, bool>>? filter = null,
                        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
}
