using System.Reflection;
using Kck.Pipeline.MediatR.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Kck.Pipeline.MediatR;

/// <summary>
/// Fluent builder for configuring MediatR pipeline behaviors.
/// </summary>
public sealed class KckPipelineBuilder
{
    /// <summary>
    /// Gets the underlying service collection.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="KckPipelineBuilder"/>.
    /// </summary>
    /// <param name="services">The service collection to register behaviors into.</param>
    public KckPipelineBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// Registers MediatR and scans the specified assembly for handlers.
    /// </summary>
    /// <param name="assembly">The assembly to scan for MediatR handlers.</param>
    /// <returns>The builder for chaining.</returns>
    public KckPipelineBuilder UseMediatR(Assembly assembly)
    {
        Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        return this;
    }

    /// <summary>
    /// Registers MediatR and scans multiple assemblies for handlers.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan for MediatR handlers.</param>
    /// <returns>The builder for chaining.</returns>
    public KckPipelineBuilder UseMediatR(params Assembly[] assemblies)
    {
        Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies));
        return this;
    }

    /// <summary>
    /// Adds the FluentValidation pipeline behavior.
    /// Validates requests before they reach the handler.
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    public KckPipelineBuilder UseValidationBehavior()
    {
        Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return this;
    }

    /// <summary>
    /// Adds the request logging pipeline behavior.
    /// Logs execution timing for requests implementing ILoggableRequest.
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    public KckPipelineBuilder UseLoggingBehavior()
    {
        Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        return this;
    }

    /// <summary>
    /// Adds the distributed cache pipeline behavior.
    /// Caches responses for requests implementing ICachableRequest.
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    public KckPipelineBuilder UseCachingBehavior()
    {
        Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        return this;
    }

    /// <summary>
    /// Adds the role-based authorization pipeline behavior.
    /// Enforces authorization for requests implementing ISecuredRequest.
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    public KckPipelineBuilder UseAuthorizationBehavior()
    {
        Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        return this;
    }

    /// <summary>
    /// Adds the database transaction pipeline behavior.
    /// Wraps handler execution in a transaction for requests implementing ITransactionalRequest.
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    public KckPipelineBuilder UseTransactionBehavior()
    {
        Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        return this;
    }
}
