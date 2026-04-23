using Kck.Core.Abstractions.Entities;
using Kck.Persistence.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Kck.Persistence.EntityFramework.Repositories;

/// <summary>
/// Factory that creates <see cref="IRepository{T, TId}"/> instances bound to a given <see cref="DbContext"/>.
/// Replaces the service-locator pattern previously used inside <c>EfUnitOfWork</c>.
/// Consumers may register a custom implementation to override default repository construction.
/// </summary>
public interface IEfRepositoryFactory
{
    IRepository<T, TId> Create<T, TId>(DbContext context) where T : Entity<TId>;
}
