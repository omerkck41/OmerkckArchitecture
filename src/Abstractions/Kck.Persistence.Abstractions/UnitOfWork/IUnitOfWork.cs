using Kck.Core.Abstractions.Entities;
using Kck.Persistence.Abstractions.Repositories;

namespace Kck.Persistence.Abstractions.UnitOfWork;

/// <summary>
/// Coordinates a single database transaction across multiple repositories, ensuring all changes are committed or rolled back atomically.
/// Implements <see cref="IAsyncDisposable"/> to guarantee transaction and connection cleanup.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>
    /// Starts a new database transaction on the underlying connection.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists all pending changes to the database. When <paramref name="autoCommitTransaction"/> is <c>true</c>,
    /// the active transaction is committed immediately after the save succeeds.
    /// </summary>
    /// <param name="autoCommitTransaction">
    /// When <c>true</c>, calls <see cref="CommitAsync"/> automatically after saving.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(bool autoCommitTransaction = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the active transaction, making all changes durable.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the active transaction, discarding all pending changes.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the repository for entity type <typeparamref name="T"/> with key type <typeparamref name="TId"/>,
    /// scoped to the current unit of work transaction.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TId">The type of the entity's primary key.</typeparam>
    IRepository<T, TId> Repository<T, TId>() where T : Entity<TId>;

    /// <summary>
    /// Releases all resources held by this unit of work, including any open transaction and database connection.
    /// </summary>
    new ValueTask DisposeAsync();
}
