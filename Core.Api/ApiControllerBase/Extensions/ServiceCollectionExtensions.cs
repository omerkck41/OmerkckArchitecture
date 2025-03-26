using Microsoft.Extensions.DependencyInjection;

namespace Core.Api.ApiControllerBase.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// API helper kütüphanesi için gerekli servisleri DI container'a ekler.
    /// </summary>
    public static IServiceCollection AddApiHelperLibrary(this IServiceCollection services)
    {
        // Örneğin, loglama veya izleme servisleri eklenebilir.
        return services;
    }
}