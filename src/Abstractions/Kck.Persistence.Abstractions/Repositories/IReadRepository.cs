using System.Linq.Expressions;
using Kck.Core.Abstractions.Entities;
using Kck.Core.Abstractions.Paging;

namespace Kck.Persistence.Abstractions.Repositories;

/// <summary>
/// Read-only repository operations. Use when write access is not needed (ISP compliance).
/// </summary>
public interface IReadRepository<T, TId> : IQuery<T> where T : Entity<TId>
{
    Task<T?> GetAsync(Expression<Func<T, bool>> predicate,
                      Expression<Func<T, object>>[]? includes = null,
                      bool withDeleted = false,
                      bool enableTracking = true,
                      CancellationToken cancellationToken = default);

    Task<IPaginate<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null,
                                    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                    Expression<Func<T, object>>[]? includes = null,
                                    int index = 0, int size = 10,
                                    bool withDeleted = false,
                                    bool enableTracking = true,
                                    CancellationToken cancellationToken = default);

    Task<IPaginate<T>> GetListByDynamicAsync(Dynamic.DynamicQuery dynamic,
                                             Expression<Func<T, bool>>? predicate = null,
                                             Expression<Func<T, object>>[]? includes = null,
                                             int index = 0, int size = 10,
                                             bool withDeleted = false,
                                             bool enableTracking = true,
                                             CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null,
                        bool withDeleted = false,
                        bool enableTracking = false,
                        CancellationToken cancellationToken = default);

    Task<T?> GetByIdAsync(TId id,
                          bool withDeleted = false,
                          bool enableTracking = false,
                          CancellationToken cancellationToken = default);

    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null,
                         bool withDeleted = false,
                         bool enableTracking = false,
                         CancellationToken cancellationToken = default);
}
