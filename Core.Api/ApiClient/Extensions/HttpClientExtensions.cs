using Core.Api.ApiClient.Models;
using Core.Api.ApiClient.Services.Implementations;
using Core.Api.ApiClient.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;
using System.Net;

namespace Core.Api.ApiClient.Extensions;

public static class HttpClientExtensions
{
    public static IServiceCollection AddApiClient(this IServiceCollection services, string baseAddress, ApiClientSettings settings)
    {
        // Jitter ekleyerek bekleme sürelerini hesaplıyoruz.
        var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(settings.BaseDelaySeconds), settings.RetryCount);

        // Retry politikası: Sadece geçici hatalar için (HTTP 5xx ve RequestTimeout gibi)
        // Unauthorized (401) ve BadRequest (400) gibi kalıcı hatalarda retry yapılmıyor.
        var retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(response =>
                ((int)response.StatusCode >= 500 || response.StatusCode == HttpStatusCode.RequestTimeout)
                && response.StatusCode != HttpStatusCode.Unauthorized && response.StatusCode != HttpStatusCode.BadRequest)
            .WaitAndRetryAsync(delay, (outcome, timespan, retryAttempt, context) =>
            {
                // Opsiyonel: Retry sırasında loglama yapabilirsiniz.
                // Örneğin: Logger.LogWarning($"Retry {retryAttempt} for {outcome.Result?.StatusCode}. Waiting {timespan} before next try.");
            });

        // Circuit breaker politikası: Belirlenen eşik değerinde tetiklenir.
        var circuitBreakerPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(response => (int)response.StatusCode >= 500)
            .CircuitBreakerAsync(settings.CircuitBreakerThreshold, TimeSpan.FromSeconds(settings.CircuitBreakerDurationSeconds));

        services.AddHttpClient<IApiClientService, ApiClientService>(client =>
        {
            client.BaseAddress = new Uri(baseAddress);
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
            client.DefaultRequestVersion = new Version(2, 0);
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreakerPolicy);

        return services;
    }
}