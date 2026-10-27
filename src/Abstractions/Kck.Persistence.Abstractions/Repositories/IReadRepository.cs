using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Kck.Core.Abstractions.Entities;
using Kck.Core.Abstractions.Paging;

namespace Kck.Persistence.Abstractions.Repositories;

/// <summary>
/// Read-only repository operations. Use when write access is not needed (ISP compliance).
/// </summary>
/// <remarks>
/// <see cref="QueryOptions"/> controls soft-deleted rows and change tracking. When <c>options</c> is <see langword="null"/>,
/// <see cref="GetAsync"/>, <see cref="GetListAsync"/> and <see cref="GetListByDynamicAsync"/> track entities
/// (<see cref="QueryOptions.Tracking"/>) and the other methods do not (<see cref="QueryOptions.None"/>).
/// Passing a value always wins: <c>default(QueryOptions)</c> and the presets (<see cref="QueryOptions.WithDeleted"/>
/// included) do not track. To load soft-deleted rows and keep tracking them, pass
/// <c>new QueryOptions(IncludeDeleted: true, AsTracking: true)</c>.
/// </remarks>
public interface IReadRepository<T, TId> : IQuery<T> where T : Entity<TId>
{
    /// <summary>Retrieves a single entity matching the predicate.</summary>
    Task<T?> GetAsync(Expression<Func<T, bool>> predicate,
                      QueryOptions? options = null,
                      Expression<Func<T, object>>[]? includes = null,
                      CancellationToken cancellationToken = default);

    /// <summary>Returns a paginated list of entities.</summary>
    [SuppressMessage("Design", "CA1005:Avoid excessive parameters on generic types", Justification = "Mirrors IQueryable composition: filter, order, includes and paging.")]
    Task<IPaginate<T>> GetListAsync(QueryOptions? options = null, // NOSONAR S107 - filter, order, includes and paging are independent knobs
                                    Expression<Func<T, bool>>? predicate = null,
                                    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                    Expression<Func<T, object>>[]? includes = null,
                                    int index = 0, int size = 10,
                                    CancellationToken cancellationToken = default);

    /// <summary>Returns a paginated list filtered by a <see cref="Dynamic.DynamicQuery"/>.</summary>
    [SuppressMessage("Design", "CA1005:Avoid excessive parameters on generic types", Justification = "Mirrors IQueryable composition: filter, includes and paging.")]
    Task<IPaginate<T>> GetListByDynamicAsync(Dynamic.DynamicQuery dynamic, // NOSONAR S107 - filter, includes and paging are independent knobs
                                             QueryOptions? options = null,
                                             Expression<Func<T, bool>>? predicate = null,
                                             Expression<Func<T, object>>[]? includes = null,
                                             int index = 0, int size = 10,
                                             CancellationToken cancellationToken = default);

    /// <summary>Returns <c>true</c> if any entity matches the predicate.</summary>
    Task<bool> AnyAsync(QueryOptions? options = null,
                        Expression<Func<T, bool>>? predicate = null,
                        CancellationToken cancellationToken = default);

    /// <summary>Retrieves a single entity by its primary key.</summary>
    Task<T?> GetByIdAsync(TId id,
                          QueryOptions? options = null,
                          CancellationToken cancellationToken = default);

    /// <summary>Returns the count of entities matching the predicate.</summary>
    Task<int> CountAsync(QueryOptions? options = null,
                         Expression<Func<T, bool>>? predicate = null,
                         CancellationToken cancellationToken = default);
}
