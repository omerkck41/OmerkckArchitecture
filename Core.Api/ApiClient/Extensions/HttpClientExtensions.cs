using Core.Api.ApiClient.Services.Implementations;
using Core.Api.ApiClient.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Core.Api.ApiClient.Extensions;

public static class HttpClientExtensions
{
    public static IServiceCollection AddApiClient(this IServiceCollection services, string baseAddress)
    {
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    // İsteğe bağlı loglama yapılabilir.
                });

        // Circuit breaker süresini 30 saniyeye indirerek daha hızlı açılması sağlandı.
        var circuitBreakerPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));

        services.AddHttpClient<IApiClientService, ApiClientService>(client =>
        {
            client.BaseAddress = new Uri(baseAddress);
            client.Timeout = TimeSpan.FromSeconds(20);
            // HTTP/2 desteği etkinleştirildi.
            client.DefaultRequestVersion = new Version(2, 0);
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreakerPolicy);

        return services;
    }
}