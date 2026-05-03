using Kck.Pipeline.Mediator.Behaviors;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Kck.Pipeline.Mediator;

/// <summary>
/// Fluent builder for registering KCK Mediator pipeline behaviors.
/// </summary>
public sealed class KckMediatorPipelineBuilder(IServiceCollection services)
{
    /// <summary>Gets the underlying service collection.</summary>
    public IServiceCollection Services { get; } = services;

    /// <summary>Registers <see cref="ValidationBehavior{TMessage,TResponse}"/> for all messages.</summary>
    public KckMediatorPipelineBuilder UseValidationBehavior()
    {
        Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return this;
    }

    /// <summary>Registers <see cref="LoggingBehavior{TMessage,TResponse}"/> for <c>ILoggableRequest</c> messages.</summary>
    public KckMediatorPipelineBuilder UseLoggingBehavior()
    {
        Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        return this;
    }

    /// <summary>Registers <see cref="CachingBehavior{TMessage,TResponse}"/> for <c>ICachableRequest</c> messages.</summary>
    public KckMediatorPipelineBuilder UseCachingBehavior()
    {
        Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        return this;
    }

    /// <summary>Registers <see cref="AuthorizationBehavior{TMessage,TResponse}"/> for <c>ISecuredRequest</c> messages.</summary>
    public KckMediatorPipelineBuilder UseAuthorizationBehavior()
    {
        Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        return this;
    }

    /// <summary>Registers <see cref="TransactionBehavior{TMessage,TResponse}"/> for <c>ITransactionalRequest</c> messages.</summary>
    public KckMediatorPipelineBuilder UseTransactionBehavior()
    {
        Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        return this;
    }
}
