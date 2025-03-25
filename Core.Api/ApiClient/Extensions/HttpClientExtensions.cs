using Core.Api.ApiClient.Services.Implementations;
using Core.Api.ApiClient.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using System.Net;

namespace Core.Api.ApiClient.Extensions;

public static class HttpClientExtensions
{
    public static IServiceCollection AddApiClient(this IServiceCollection services, string baseAddress)
    {
        // Transient hatalar için geliştirilen retry policy.
        // 401 Unauthorized veya 400 BadRequest gibi hata durumları yeniden denemeye dahil edilmeyecek.
        var retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(response =>
                // Yalnızca HTTP 5xx hataları veya RequestTimeout (408) gibi durumlarda retry uygulanır.
                ((int)response.StatusCode >= 500 || response.StatusCode == HttpStatusCode.RequestTimeout)
                && response.StatusCode != HttpStatusCode.Unauthorized)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    // Opsiyonel: Loglama veya başka işlemler yapılabilir.
                });

        // Circuit breaker politikasını da aynı şekilde yapılandırıyoruz.
        var circuitBreakerPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(response => (int)response.StatusCode >= 500)
            .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));

        services.AddHttpClient<IApiClientService, ApiClientService>(client =>
        {
            client.BaseAddress = new Uri(baseAddress);
            client.Timeout = TimeSpan.FromSeconds(20);
            // HTTP/2 desteği etkinleştiriliyor.
            client.DefaultRequestVersion = new Version(2, 0);
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreakerPolicy);

        return services;
    }
}