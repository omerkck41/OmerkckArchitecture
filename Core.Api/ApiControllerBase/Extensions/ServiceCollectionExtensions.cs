using Microsoft.Extensions.DependencyInjection;

namespace Core.Api.ApiControllerBase.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiHelperLibrary(this IServiceCollection services)
    {
        // Gerekirse bağımlılıklar burada eklenir.
        return services;
    }
}