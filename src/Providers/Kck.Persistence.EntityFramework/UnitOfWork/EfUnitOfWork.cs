using System.Collections.Concurrent;
using Kck.Core.Abstractions.Entities;
using Kck.Persistence.Abstractions.Repositories;
using Kck.Persistence.Abstractions.UnitOfWork;
using Kck.Persistence.EntityFramework.Interceptors;
using Kck.Persistence.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Kck.Persistence.EntityFramework.UnitOfWork;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/>.
/// </summary>
public sealed class EfUnitOfWork(DbContext context, IEfRepositoryFactory repositoryFactory) : IUnitOfWork
{
    private readonly ConcurrentDictionary<string, object> _repositories = new();
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction ??= await context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(
        bool autoCommitTransaction = false,
        CancellationToken cancellationToken = default)
    {
        var result = await context.SaveChangesAsync(cancellationToken);

        if (autoCommitTransaction && _transaction is not null)
            await CommitCurrentAsync(_transaction, cancellationToken);

        return result;
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No active transaction. Call BeginTransactionAsync first.");

        await CommitCurrentAsync(_transaction, cancellationToken);
    }

    // Whether the commit succeeds or fails, the transaction is gone when this returns, so callers never roll back after
    // CommitAsync. A successful commit releases it before the events are dispatched, so a dispatch failure leaves the
    // unit of work ready for the next transaction.
    private async Task CommitCurrentAsync(IDbContextTransaction transaction, CancellationToken cancellationToken)
    {
        try
        {
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackAfterFailedCommitAsync(transaction);
            await ReleaseTransactionAsync();
            throw;
        }

        await ReleaseTransactionAsync();
        await DispatchCommittedEventsAsync(cancellationToken);
    }

    private static async Task RollbackAfterFailedCommitAsync(IDbContextTransaction transaction)
    {
        try
        {
            await transaction.RollbackAsync(CancellationToken.None);
        }
        catch (Exception rollbackError) when (rollbackError is not OutOfMemoryException) // NOSONAR S2486 - the commit error is rethrown by the caller; a failing rollback adds nothing to it
        {
            // The commit error is the one the caller needs; a rollback that also fails changes nothing for it.
        }
    }

    private async Task ReleaseTransactionAsync()
    {
        if (_transaction is not { } transaction)
            return;

        _transaction = null;
        await transaction.DisposeAsync();
    }

    // Domain events saved inside the transaction are dispatched only now, after EF has released it.
    private Task DispatchCommittedEventsAsync(CancellationToken cancellationToken) =>
        context.GetService<IDbContextOptions>().FindExtension<CoreOptionsExtension>()?.Interceptors?
            .OfType<DomainEventDispatchInterceptor>().FirstOrDefault() is { } interceptor
            ? interceptor.DispatchCommittedAsync(context, cancellationToken)
            : Task.CompletedTask;

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No active transaction. Call BeginTransactionAsync first.");

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await ReleaseTransactionAsync();
        }
    }

    /// <inheritdoc />
    public IRepository<T, TId> Repository<T, TId>() where T : Entity<TId>
    {
        var key = $"{typeof(T).FullName}_{typeof(TId).FullName}";

        return (IRepository<T, TId>)_repositories.GetOrAdd(
            key,
            _ => repositoryFactory.Create<T, TId>(context));
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        await ReleaseTransactionAsync();
        _disposed = true;
    }
}
