using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Resilience;
using Polly;

namespace Kck.Resilience.Polly;

/// <summary>
/// Extension methods for registering Polly-backed resilience pipelines via
/// <c>Microsoft.Extensions.Resilience</c>.
/// </summary>
public static class KckResilienceServiceCollectionExtensions
{
    /// <summary>
    /// Registers a named resilience pipeline. Inject
    /// <c>ResiliencePipelineProvider&lt;string&gt;</c>
    /// to resolve pipelines by name at runtime.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="pipelineName">Name used to resolve the pipeline.</param>
    /// <param name="configure">Delegate to configure retry, circuit-breaker, and timeout.</param>
    /// <example>
    /// <code>
    /// services.AddKckResilience("external-api", pipeline =>
    /// {
    ///     pipeline.AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 3 });
    ///     pipeline.AddCircuitBreaker(new CircuitBreakerStrategyOptions());
    ///     pipeline.AddTimeout(TimeSpan.FromSeconds(10));
    /// });
    ///
    /// // Resolution:
    /// var provider = serviceProvider.GetRequiredService&lt;ResiliencePipelineProvider&lt;string&gt;&gt;();
    /// var pipeline = provider.GetPipeline("external-api");
    /// await pipeline.ExecuteAsync(ct => DoWorkAsync(ct));
    /// </code>
    /// </example>
    public static IServiceCollection AddKckResilience(
        this IServiceCollection services,
        string pipelineName,
        Action<ResiliencePipelineBuilder> configure)
    {
        services.AddResiliencePipeline(pipelineName, configure);
        return services;
    }

    /// <summary>
    /// Registers a named resilience pipeline for typed results.
    /// </summary>
    public static IServiceCollection AddKckResilience<TResult>(
        this IServiceCollection services,
        string pipelineName,
        Action<ResiliencePipelineBuilder<TResult>> configure)
    {
        services.AddResiliencePipeline<string, TResult>(pipelineName, configure);
        return services;
    }
}
