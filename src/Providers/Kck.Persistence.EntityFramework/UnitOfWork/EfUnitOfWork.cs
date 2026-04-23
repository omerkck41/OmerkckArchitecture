using System.Collections.Concurrent;
using Kck.Core.Abstractions.Entities;
using Kck.Persistence.Abstractions.Repositories;
using Kck.Persistence.Abstractions.UnitOfWork;
using Kck.Persistence.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
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
            await _transaction.CommitAsync(cancellationToken);

        return result;
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No active transaction. Call BeginTransactionAsync first.");

        await _transaction.CommitAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No active transaction. Call BeginTransactionAsync first.");

        await _transaction.RollbackAsync(cancellationToken);
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

        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        _disposed = true;
    }
}
