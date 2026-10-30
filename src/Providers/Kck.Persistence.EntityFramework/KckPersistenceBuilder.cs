using System.Reflection;
using Kck.Core.Abstractions.Entities;
using Kck.Persistence.Abstractions.UnitOfWork;
using Kck.Persistence.EntityFramework.Interceptors;
using Kck.Persistence.EntityFramework.Repositories;
using Kck.Persistence.EntityFramework.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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
    /// Registers <typeparamref name="TContext"/> with DbContext pooling (<c>AddDbContextPool</c>) and wires up
    /// <see cref="IUnitOfWork"/> as <see cref="UseEntityFramework{TContext}"/> does. Pooled contexts share one options
    /// instance: the audit interceptor is a singleton anyway, and domain events are dispatched by a dispatcher resolved
    /// from a new scope for each dispatch, so a handler saving through a DbContext gets its own instance.
    /// </summary>
    /// <param name="configure">Configures the shared options (built once, from the root service provider).</param>
    /// <param name="poolSize">Maximum number of contexts kept by the pool.</param>
    public KckPersistenceBuilder UseEntityFrameworkPool<TContext>(
        Action<DbContextOptionsBuilder> configure, int poolSize = 1024) where TContext : DbContext
    {
        Services.AddDbContextPool<TContext>((sp, options) =>
        {
            configure(options);
            AddRegisteredInterceptor<AuditInterceptor>(sp, options);
            // The collection, not IServiceProviderIsService: works with any container (registration is complete here).
            if (Services.Any(d => d.ServiceType == typeof(IDomainEventDispatcher)))
            {
                AddInterceptor(options, new DomainEventDispatchInterceptor(
                    sp.GetRequiredService<IServiceScopeFactory>(),
                    sp.GetService<ILogger<DomainEventDispatchInterceptor>>() ?? NullLogger<DomainEventDispatchInterceptor>.Instance));
            }
        }, poolSize);
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
    /// <exception cref="InvalidOperationException"><see cref="AddDomainEventHandlers"/> was called before.</exception>
    public KckPersistenceBuilder AddDomainEventDispatch<TDispatcher>()
        where TDispatcher : class, IDomainEventDispatcher
    {
        if (RegisteredDispatcher() is { } existing && IsHandlerDispatcher(existing))
        {
            throw new InvalidOperationException(
                $"AddDomainEventDispatch<{typeof(TDispatcher).Name}>() cannot be combined with AddDomainEventHandlers(...): " +
                "dispatch domain events either through IDomainEventHandler<T> handlers or through a custom IDomainEventDispatcher.");
        }

        AddDispatcher<TDispatcher>();
        return this;
    }

    /// <summary>
    /// Registers every concrete, non-generic <see cref="IDomainEventHandler{TEvent}"/> implementation in
    /// <paramref name="assemblies"/> as a scoped service and dispatches domain events to them (ADR-0030). A handler
    /// receives events of exactly its event type; the order of several handlers for one event is not defined.
    /// </summary>
    /// <param name="assemblies">Assemblies to scan; a handler type found twice is registered once.</param>
    /// <exception cref="ArgumentException"><paramref name="assemblies"/> is empty or contains <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">
    /// Another <see cref="IDomainEventDispatcher"/> is registered (for example with
    /// <see cref="AddDomainEventDispatch{TDispatcher}"/>).
    /// </exception>
    public KckPersistenceBuilder AddDomainEventHandlers(params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        if (assemblies.Length == 0)
            throw new ArgumentException("Pass at least one assembly to scan for IDomainEventHandler<T> implementations.", nameof(assemblies));
        if (Array.Exists(assemblies, a => a is null))
            throw new ArgumentException("The assemblies to scan must not contain null.", nameof(assemblies));

        var dispatcher = RegisteredDispatcher();
        if (dispatcher is not null && !IsHandlerDispatcher(dispatcher))
        {
            throw new InvalidOperationException(
                $"AddDomainEventHandlers(...) cannot be combined with the IDomainEventDispatcher already registered " +
                $"({Describe(dispatcher)}, for example by AddDomainEventDispatch<T>()): dispatch domain events either through " +
                "IDomainEventHandler<T> handlers or through a custom IDomainEventDispatcher.");
        }

        var handlerTypes = assemblies.SelectMany(LoadableTypes)
            .Where(type => type.IsClass && !type.IsAbstract && !type.ContainsGenericParameters);
        foreach (var type in handlerTypes)
        {
            foreach (var handlerInterface in type.GetInterfaces().Where(IsDomainEventHandlerInterface))
                Services.TryAddEnumerable(ServiceDescriptor.Scoped(handlerInterface, type));
        }

        if (dispatcher is null)
            AddDispatcher<ServiceProviderDomainEventDispatcher>();
        return this;
    }

    private void AddDispatcher<TDispatcher>() where TDispatcher : class, IDomainEventDispatcher
    {
        Services.AddScoped<IDomainEventDispatcher, TDispatcher>();
        Services.AddScoped<DomainEventDispatchInterceptor>();
    }

    // Keyed registrations are not used by the interceptor, so they neither conflict nor count.
    private ServiceDescriptor? RegisteredDispatcher() =>
        Services.LastOrDefault(d => d.ServiceType == typeof(IDomainEventDispatcher) && !d.IsKeyedService);

    private static bool IsDomainEventHandlerInterface(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>);

    private static bool IsHandlerDispatcher(ServiceDescriptor descriptor) =>
        descriptor.ImplementationType == typeof(ServiceProviderDomainEventDispatcher);

    private static string Describe(ServiceDescriptor descriptor) =>
        (descriptor.ImplementationType ?? descriptor.ImplementationInstance?.GetType())?.Name ?? "registered through a factory";

    // A type that cannot be loaded (e.g. it references an assembly that is not deployed) must not stop the scan.
    private static IEnumerable<Type> LoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.OfType<Type>();
        }
    }

    private static void AddRegisteredInterceptor<TInterceptor>(
        IServiceProvider serviceProvider, DbContextOptionsBuilder options)
        where TInterceptor : class, IInterceptor
    {
        var interceptor = serviceProvider.GetService<TInterceptor>();
        if (interceptor is not null)
            AddInterceptor(options, interceptor);
    }

    private static void AddInterceptor<TInterceptor>(DbContextOptionsBuilder options, TInterceptor interceptor)
        where TInterceptor : class, IInterceptor
    {
        var existing = options.Options.FindExtension<CoreOptionsExtension>()?.Interceptors;
        if (existing is not null && existing.OfType<TInterceptor>().Any())
            return;

        options.AddInterceptors(interceptor);
    }
}
