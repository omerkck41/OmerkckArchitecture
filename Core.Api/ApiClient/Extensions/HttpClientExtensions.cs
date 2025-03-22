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
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        var circuitBreakerPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        services.AddHttpClient<IApiClientService, ApiClientService>(client =>
        {
            client.BaseAddress = new Uri(baseAddress);
            client.Timeout = TimeSpan.FromSeconds(20);
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreakerPolicy);

        return services;
    }
}