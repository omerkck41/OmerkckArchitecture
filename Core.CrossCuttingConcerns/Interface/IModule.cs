using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.CrossCuttingConcerns.Interface;

/// <summary>
/// Tüm modüllerin implement etmesi gereken temel arayüz.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Modülün servislerini ve bağımlılıklarını ekler.
    /// </summary>
    /// <param name="services">Servis koleksiyonu</param>
    /// <param name="configuration">Uygulama konfigürasyonu</param>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
}