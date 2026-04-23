using Kck.Core.Abstractions.Entities;
using Kck.Persistence.Abstractions.Repositories;

namespace Kck.Persistence.Abstractions.UnitOfWork;

public interface IUnitOfWork : IAsyncDisposable
{
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(bool autoCommitTransaction = false, CancellationToken cancellationToken = default);

    Task CommitAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);

    IRepository<T, TId> Repository<T, TId>() where T : Entity<TId>;

    new ValueTask DisposeAsync();
}
