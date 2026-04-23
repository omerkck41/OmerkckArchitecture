using Kck.Pipeline.MediatR;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering KCK MediatR pipeline behaviors.
/// </summary>
public static class KckPipelineMediatRServiceCollectionExtensions
{
    /// <summary>
    /// Adds KCK MediatR pipeline with builder pattern configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure pipeline behaviors via <see cref="KckPipelineBuilder"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddKckPipeline(
        this IServiceCollection services,
        Action<KckPipelineBuilder> configure)
    {
        var builder = new KckPipelineBuilder(services);
        configure(builder);
        return services;
    }
}
