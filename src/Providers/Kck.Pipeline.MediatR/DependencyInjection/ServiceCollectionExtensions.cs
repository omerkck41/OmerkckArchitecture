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
    /// <remarks>
    /// <b>Deprecated (KCK0200):</b> Migrate to <c>AddKckMediator()</c> from
    /// <c>Kck.Pipeline.Mediator</c>. See docs/migrations/mediatR-to-mediator.md.
    /// </remarks>
    [Obsolete(
        "Kck.Pipeline.MediatR is deprecated (KCK0200). " +
        "Use AddKckMediator() from Kck.Pipeline.Mediator instead. " +
        "Migration guide: docs/migrations/mediatR-to-mediator.md",
        error: false,
        DiagnosticId = "KCK0200",
        UrlFormat = "https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/migrations/mediatR-to-mediator.md")]
    public static IServiceCollection AddKckPipeline(
        this IServiceCollection services,
        Action<KckPipelineBuilder> configure)
    {
        var builder = new KckPipelineBuilder(services);
        configure(builder);
        return services;
    }
}
