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

        services.AddHttpClient<IApiClient, ResilientApiClient>()
            .AddStandardResilienceHandler(handler =>
            {
                handler.Retry.MaxRetryAttempts = options.MaxRetryAttempts;
                handler.Retry.Delay = options.MedianFirstRetryDelay;
                handler.CircuitBreaker.SamplingDuration = options.CircuitBreakerDuration * 2;
                handler.AttemptTimeout.Timeout = options.Timeout;
                handler.TotalRequestTimeout.Timeout = options.Timeout * 3;
            });

        return services;
    }
}
