using Core.Integration.Interfaces;
using Core.Integration.Models;
using Core.Integration.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Integration.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIntegrationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ThirdPartyOptions>(configuration.GetSection("ThirdPartySettings"));
        services.AddHttpClient<IApiClient, ApiClient>();
        services.AddScoped<IThirdPartyIntegrationService, ThirdPartyIntegrationService>();
        return services;
    }
}