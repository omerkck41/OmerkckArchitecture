using Kck.Persistence.Abstractions.UnitOfWork;
using Kck.Persistence.EntityFramework.Interceptors;
using Kck.Persistence.EntityFramework.Repositories;
using Kck.Persistence.EntityFramework.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kck.Persistence.EntityFramework;

/// <summary>
/// Fluent builder for configuring Kck persistence services.
/// </summary>
public sealed class KckPersistenceBuilder(IServiceCollection services)
{
    /// <summary>
    /// Gets the underlying service collection.
    /// </summary>
    public IServiceCollection Services { get; } = services;

    /// <summary>
    /// Registers the specified <typeparamref name="TContext"/> as the EF Core DbContext
    /// and wires up <see cref="IUnitOfWork"/>.
    /// </summary>
    public KckPersistenceBuilder UseEntityFramework<TContext>(
        Action<DbContextOptionsBuilder> configure) where TContext : DbContext
    {
        Services.AddDbContext<TContext>(configure);
        Services.AddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());
        Services.TryAddScoped<IEfRepositoryFactory, DefaultEfRepositoryFactory>();
        Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        return this;
    }

    /// <summary>
    /// Registers the <see cref="AuditInterceptor"/> as a singleton.
    /// </summary>
    public KckPersistenceBuilder AddAuditInterceptor()
    {
        Services.TryAddSingleton<AuditInterceptor>();
        return this;
    }

    /// <summary>
    /// Registers the <see cref="IDomainEventDispatcher"/> implementation
    /// and the <see cref="DomainEventDispatchInterceptor"/>.
    /// </summary>
    public KckPersistenceBuilder AddDomainEventDispatch<TDispatcher>()
        where TDispatcher : class, IDomainEventDispatcher
    {
        Services.AddScoped<IDomainEventDispatcher, TDispatcher>();
        Services.AddScoped<DomainEventDispatchInterceptor>();
        return this;
    }
}
