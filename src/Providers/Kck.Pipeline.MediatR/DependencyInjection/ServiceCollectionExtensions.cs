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
#pragma warning disable S1133 // Deprecated code kept for backward compatibility — removal planned for v3.0
    [Obsolete(
        "Kck.Pipeline.MediatR is deprecated (KCK0200). " +
        "Use AddKckMediator() from Kck.Pipeline.Mediator instead. " +
        "Migration guide: docs/migrations/mediatR-to-mediator.md",
        error: false,
        DiagnosticId = "KCK0200",
        UrlFormat = "https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/migrations/mediatR-to-mediator.md")]
#pragma warning restore S1133
    public static IServiceCollection AddKckPipeline(
        this IServiceCollection services,
        Action<KckPipelineBuilder> configure)
    {
#pragma warning disable KCK0200 // internal usage within the same deprecated package
        var builder = new KckPipelineBuilder(services);
#pragma warning restore KCK0200
        configure(builder);
        return services;
    }
}
