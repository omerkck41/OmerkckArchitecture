using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Kck.Core.Abstractions.Entities;
using Kck.Core.Abstractions.Paging;

namespace Kck.Persistence.Abstractions.Repositories;

/// <summary>
/// Read-only repository operations. Use when write access is not needed (ISP compliance).
/// </summary>
public interface IReadRepository<T, TId> : IQuery<T> where T : Entity<TId>
{
    /// <summary>Retrieves a single entity matching the predicate.</summary>
    [Obsolete("KCK0100: Use ReadRepositoryExtensions.GetAsync with QueryOptions instead.", DiagnosticId = "KCK0100")]
    Task<T?> GetAsync(Expression<Func<T, bool>> predicate,
                      Expression<Func<T, object>>[]? includes = null,
                      bool withDeleted = false,
                      bool enableTracking = true,
                      CancellationToken cancellationToken = default);

    /// <summary>Returns a paginated list of entities.</summary>
    [Obsolete("KCK0100: Use ReadRepositoryExtensions.GetListAsync with QueryOptions instead.", DiagnosticId = "KCK0100")]
    [SuppressMessage("Design", "CA1005:Avoid excessive parameters on generic types", Justification = "Deprecated API retained for backwards compatibility — replaced by QueryOptions overloads.")]
    Task<IPaginate<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null,
                                    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                    Expression<Func<T, object>>[]? includes = null,
                                    int index = 0, int size = 10,
                                    bool withDeleted = false,
                                    bool enableTracking = true,
                                    CancellationToken cancellationToken = default);

    /// <summary>Returns a paginated list filtered by a <see cref="Dynamic.DynamicQuery"/>.</summary>
    [Obsolete("KCK0100: Use ReadRepositoryExtensions.GetListByDynamicAsync with QueryOptions instead.", DiagnosticId = "KCK0100")]
    [SuppressMessage("Design", "CA1005:Avoid excessive parameters on generic types", Justification = "Deprecated API retained for backwards compatibility — replaced by QueryOptions overloads.")]
    Task<IPaginate<T>> GetListByDynamicAsync(Dynamic.DynamicQuery dynamic,
                                             Expression<Func<T, bool>>? predicate = null,
                                             Expression<Func<T, object>>[]? includes = null,
                                             int index = 0, int size = 10,
                                             bool withDeleted = false,
                                             bool enableTracking = true,
                                             CancellationToken cancellationToken = default);

    /// <summary>Returns <c>true</c> if any entity matches the predicate.</summary>
    [Obsolete("KCK0100: Use ReadRepositoryExtensions.AnyAsync with QueryOptions instead.", DiagnosticId = "KCK0100")]
    Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null,
                        bool withDeleted = false,
                        bool enableTracking = false,
                        CancellationToken cancellationToken = default);

    /// <summary>Retrieves a single entity by its primary key.</summary>
    [Obsolete("KCK0100: Use ReadRepositoryExtensions.GetByIdAsync with QueryOptions instead.", DiagnosticId = "KCK0100")]
    Task<T?> GetByIdAsync(TId id,
                          bool withDeleted = false,
                          bool enableTracking = false,
                          CancellationToken cancellationToken = default);

    /// <summary>Returns the count of entities matching the predicate.</summary>
    [Obsolete("KCK0100: Use ReadRepositoryExtensions.CountAsync with QueryOptions instead.", DiagnosticId = "KCK0100")]
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null,
                         bool withDeleted = false,
                         bool enableTracking = false,
                         CancellationToken cancellationToken = default);
}
