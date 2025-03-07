using Microsoft.Extensions.DependencyInjection;

namespace Core.Api.ApiHelperLibrary.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiHelperLibrary(this IServiceCollection services)
    {
        // Gerekirse bağımlılıklar burada eklenir.
        return services;
    }
}