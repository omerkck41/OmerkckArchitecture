using Kck.Core.Abstractions.Entities;

namespace Kck.Persistence.Abstractions.Repositories;

/// <summary>
/// Full repository combining read and write operations.
/// For ISP compliance, prefer injecting <see cref="IReadRepository{T, TId}"/>
/// or <see cref="IWriteRepository{T, TId}"/> when only a subset of operations is needed.
/// </summary>
public interface IRepository<T, TId> : IReadRepository<T, TId>, IWriteRepository<T, TId>
    where T : Entity<TId>;
