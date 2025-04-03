using Core.CrossCuttingConcerns.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.CrossCuttingConcerns.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Belirtilen assembly(ler) içerisindeki tüm IModule implementasyonlarını bulur ve servis koleksiyonuna ekler.
    /// </summary>
    /// <param name="services">Servis koleksiyonu</param>
    /// <param name="configuration">Uygulama konfigürasyonu</param>
    /// <param name="assemblies">Taranacak assembly(ler)</param>
    /// <returns>Güncellenmiş servis koleksiyonu</returns>
    public static IServiceCollection AddModules(this IServiceCollection services, IConfiguration configuration, params Assembly[] assemblies)
    {
        var moduleTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToList();

        foreach (var moduleType in moduleTypes)
        {
            var module = Activator.CreateInstance(moduleType) as IModule;
            module?.ConfigureServices(services, configuration);
        }

        return services;
    }
}