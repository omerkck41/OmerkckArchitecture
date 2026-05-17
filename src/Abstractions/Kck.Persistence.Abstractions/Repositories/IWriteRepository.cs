using System.Linq.Expressions;
using Kck.Core.Abstractions.Entities;

namespace Kck.Persistence.Abstractions.Repositories;

/// <summary>
/// Write-only repository operations. Use when read access is not needed (ISP compliance).
/// </summary>
public interface IWriteRepository<T, TId> where T : Entity<TId>
{
    /// <summary>Adds a single entity and returns it after persistence.</summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Adds a collection of entities and returns them after persistence.</summary>
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>Updates all tracked properties of the entity.</summary>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Updates only the specified properties of the entity.</summary>
    Task<T> UpdatePartialAsync(T entity, Expression<Func<T, object>>[] properties, CancellationToken cancellationToken = default);

    /// <summary>Applies property assignments in bulk to all entities matching the predicate.</summary>
    Task BulkUpdateAsync(Expression<Func<T, bool>> predicate, (Expression<Func<T, object>> Property, object? Value)[] updates, CancellationToken cancellationToken = default);

    /// <summary>Updates a collection of entities.</summary>
    Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>Deletes the entity; soft-deletes by default unless <paramref name="permanent"/> is <c>true</c>.</summary>
    Task<T> DeleteAsync(T entity, bool permanent = false, CancellationToken cancellationToken = default);

    /// <summary>Restores a soft-deleted entity by clearing its deletion marker.</summary>
    Task<T> RevertSoftDeleteAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Deletes the first entity matching the predicate; soft-deletes by default unless <paramref name="permanent"/> is <c>true</c>.</summary>
    Task<T> DeleteAsync(Expression<Func<T, bool>> predicate, bool permanent = false, CancellationToken cancellationToken = default);

    /// <summary>Deletes the entity with the given primary key; soft-deletes by default unless <paramref name="permanent"/> is <c>true</c>.</summary>
    Task<T> DeleteAsync(TId id, bool permanent = false, CancellationToken cancellationToken = default);

    /// <summary>Restores a soft-deleted entity identified by its primary key.</summary>
    Task<T> RevertSoftDeleteAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>Deletes a collection of entities; soft-deletes by default unless <paramref name="permanent"/> is <c>true</c>.</summary>
    Task DeleteRangeAsync(IEnumerable<T> entities, bool permanent = false, CancellationToken cancellationToken = default);
}
