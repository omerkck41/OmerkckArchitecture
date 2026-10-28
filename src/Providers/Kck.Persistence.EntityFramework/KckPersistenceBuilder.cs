using Kck.Persistence.Abstractions.UnitOfWork;
using Kck.Persistence.EntityFramework.Interceptors;
using Kck.Persistence.EntityFramework.Repositories;
using Kck.Persistence.EntityFramework.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
    /// and wires up <see cref="IUnitOfWork"/>. Interceptors registered through
    /// <see cref="AddAuditInterceptor"/> or <see cref="AddDomainEventDispatch{TDispatcher}"/>
    /// are added to the context options unless <paramref name="configure"/> already added one of the same type.
    /// </summary>
    public KckPersistenceBuilder UseEntityFramework<TContext>(
        Action<DbContextOptionsBuilder> configure) where TContext : DbContext
    {
        Services.AddDbContext<TContext>((sp, options) =>
        {
            configure(options);
            AddRegisteredInterceptor<AuditInterceptor>(sp, options);
            AddRegisteredInterceptor<DomainEventDispatchInterceptor>(sp, options);
        });
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

    private static void AddRegisteredInterceptor<TInterceptor>(
        IServiceProvider serviceProvider, DbContextOptionsBuilder options)
        where TInterceptor : class, IInterceptor
    {
        var interceptor = serviceProvider.GetService<TInterceptor>();
        if (interceptor is null)
            return;

        var existing = options.Options.FindExtension<CoreOptionsExtension>()?.Interceptors;
        if (existing is not null && existing.OfType<TInterceptor>().Any())
            return;

        options.AddInterceptors((IInterceptor)interceptor);
    }
}
