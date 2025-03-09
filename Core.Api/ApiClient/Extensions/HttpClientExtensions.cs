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
        // Polly ile yeniden deneme mekanizması
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        services.AddHttpClient<IApiClientService, ApiClientService>(client =>
        {
            client.BaseAddress = new Uri(baseAddress);
            client.Timeout = TimeSpan.FromSeconds(10);
        })
        .AddPolicyHandler(retryPolicy);

        return services;
    }
}