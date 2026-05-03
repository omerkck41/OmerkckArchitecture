using Kck.Pipeline.Mediator;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering KCK Mediator pipeline behaviors.
/// </summary>
public static class KckMediatorPipelineServiceCollectionExtensions
{
    /// <summary>
    /// Adds KCK Mediator pipeline behaviors via fluent builder.
    /// Call <c>services.AddMediator()</c> separately to register the source-generated dispatcher.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddMediator();
    /// services.AddKckMediatorPipeline(p =&gt; p
    ///     .UseValidationBehavior()
    ///     .UseLoggingBehavior()
    ///     .UseCachingBehavior());
    /// </code>
    /// </example>
    public static IServiceCollection AddKckMediatorPipeline(
        this IServiceCollection services,
        Action<KckMediatorPipelineBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);
        configure(new KckMediatorPipelineBuilder(services));
        return services;
    }
}
