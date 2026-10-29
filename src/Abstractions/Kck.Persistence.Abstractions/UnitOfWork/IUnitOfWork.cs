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
    /// <remarks>
    /// Does nothing while a transaction is already active. A transaction ends with <see cref="CommitAsync"/>
    /// (whether it succeeds or throws) or <see cref="RollbackAsync"/>; the next call then starts a new one.
    /// </remarks>
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
    /// <remarks>
    /// When this method returns or throws, the transaction has ended: if the database commit fails, a rollback is
    /// attempted and the transaction is released before the error is rethrown. Do not call <see cref="RollbackAsync"/>
    /// after a failed commit. When the commit outcome is unknown (for example the connection is lost during the
    /// commit), the changes may already be durable. An exception can also come after a successful commit (for example
    /// from domain event dispatch); the changes are then durable.
    /// </remarks>
    /// <param name="cancellationToken">
    /// Observed before the database commit starts (a canceled token rolls back and throws) and by the work that follows
    /// a successful commit, such as domain event dispatch; the database commit itself is not canceled once started
    /// (a dead connection is bounded by the provider's own timeouts). Canceled after the commit, the changes are
    /// durable and dispatch may be canceled: its events go back on their entities, so only a later save through the
    /// same context dispatches them — use an outbox when every event must be delivered.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// No transaction is active, or the provider reports the transaction unusable.
    /// </exception>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the active transaction, discarding all pending changes.
    /// </summary>
    /// <remarks>The transaction is released even when the rollback itself throws.</remarks>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="InvalidOperationException">No transaction is active.</exception>
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
