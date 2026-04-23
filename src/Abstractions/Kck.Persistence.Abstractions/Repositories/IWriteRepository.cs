using System.Linq.Expressions;
using Kck.Core.Abstractions.Entities;

namespace Kck.Persistence.Abstractions.Repositories;

/// <summary>
/// Write-only repository operations. Use when read access is not needed (ISP compliance).
/// </summary>
public interface IWriteRepository<T, TId> where T : Entity<TId>
{
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    Task<T> UpdatePartialAsync(T entity, Expression<Func<T, object>>[] properties, CancellationToken cancellationToken = default);

    Task BulkUpdateAsync(Expression<Func<T, bool>> predicate, (Expression<Func<T, object>> Property, object? Value)[] updates, CancellationToken cancellationToken = default);

    Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    Task<T> DeleteAsync(T entity, bool permanent = false, CancellationToken cancellationToken = default);

    Task<T> RevertSoftDeleteAsync(T entity, CancellationToken cancellationToken = default);

    Task<T> DeleteAsync(Expression<Func<T, bool>> predicate, bool permanent = false, CancellationToken cancellationToken = default);

    Task<T> DeleteAsync(TId id, bool permanent = false, CancellationToken cancellationToken = default);

    Task<T> RevertSoftDeleteAsync(TId id, CancellationToken cancellationToken = default);

    Task DeleteRangeAsync(IEnumerable<T> entities, bool permanent = false, CancellationToken cancellationToken = default);
}
