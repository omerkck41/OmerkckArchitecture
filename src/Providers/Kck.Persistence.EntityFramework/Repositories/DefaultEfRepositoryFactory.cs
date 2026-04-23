using Kck.Core.Abstractions.Entities;
using Kck.Persistence.Abstractions.Repositories;
using Kck.Persistence.Abstractions.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kck.Persistence.EntityFramework.Repositories;

/// <summary>
/// Default <see cref="IEfRepositoryFactory"/> implementation.
/// Prefers a DI-registered <see cref="IRepository{T, TId}"/> when available,
/// otherwise constructs <see cref="EfRepository{T, TId}"/> with an optional
/// <see cref="IFilterPropertyWhitelist{T}"/> resolved from DI.
/// </summary>
public sealed class DefaultEfRepositoryFactory(IServiceProvider serviceProvider) : IEfRepositoryFactory
{
    public IRepository<T, TId> Create<T, TId>(DbContext context) where T : Entity<TId>
    {
        var custom = serviceProvider.GetService<IRepository<T, TId>>();
        if (custom is not null)
            return custom;

        var whitelist = serviceProvider.GetService<IFilterPropertyWhitelist<T>>();
        return new EfRepository<T, TId>(context, whitelist);
    }
}
