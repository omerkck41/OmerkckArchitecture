using Kck.Http.Abstractions;
using Kck.Http.Resilience;
using Microsoft.Extensions.Http.Resilience;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckHttpResilienceServiceCollectionExtensions
{
    public static IServiceCollection AddKckHttpResilience(
        this IServiceCollection services,
        Action<ResilienceOptions>? configure = null)
    {
        var options = new ResilienceOptions();
        configure?.Invoke(options);
        // The typed client reads SerializerOptions from IOptions<ResilienceOptions>.
        services.AddOptions<ResilienceOptions>().Configure(o => configure?.Invoke(o));

        services.AddHttpClient<IApiClient, ResilientApiClient>()
            .AddStandardResilienceHandler(handler =>
            {
                handler.Retry.MaxRetryAttempts = options.MaxRetryAttempts;
                handler.Retry.Delay = options.MedianFirstRetryDelay;
                handler.CircuitBreaker.BreakDuration = options.CircuitBreakerDuration;
                handler.CircuitBreaker.FailureRatio = options.CircuitBreakerFailureRatio;
                handler.CircuitBreaker.MinimumThroughput = options.CircuitBreakerMinimumThroughput;
                // The standard handler requires a sampling window of at least twice the attempt timeout; 30 s (its
                // default) is the floor, so a very short timeout does not shrink the window below its minimum.
                handler.CircuitBreaker.SamplingDuration = TimeSpan.FromTicks(Math.Max(
                    (options.Timeout * 2).Ticks, TimeSpan.FromSeconds(30).Ticks));
                handler.AttemptTimeout.Timeout = options.Timeout;
                handler.TotalRequestTimeout.Timeout = options.Timeout * 3;
            });

        return services;
    }
}
