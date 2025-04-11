using Core.StatusHandling.Handlers;
using Core.StatusHandling.Interfaces;
using Core.StatusHandling.Middleware;
using Core.StatusHandling.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Core.StatusHandling.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Core.StatusHandling servislerini ve varsayılan işleyicileri DI container'ına ekler.
    /// </summary>
    /// <param name="services">Servis koleksiyonu.</param>
    /// <param name="configureOptions">Yapılandırma seçeneklerini ayarlamak için bir Action.</param>
    /// <returns>Yapılandırmaya devam etmek için IServiceCollection.</returns>
    public static IServiceCollection AddStatusCodeHandling(
        this IServiceCollection services,
        Action<StatusCodeHandlingOptions> configureOptions = null)
    {
        // Options pattern'i yapılandır
        // Kullanıcı bir configureOptions aksiyonu sağladıysa onu çalıştır, sağlamadıysa varsayılanları kullanır.
        var optionsBuilder = services.AddOptions<StatusCodeHandlingOptions>();
        if (configureOptions != null)
        {
            optionsBuilder.Configure(configureOptions);
        }

        // Varsayılan Handler'ları Scoped olarak ekle.
        // TryAddScoped: Eğer aynı servis tipi daha önce eklenmemişse ekler.
        // Bu, kullanıcının kendi özel handler'ını aynı arayüzle kaydetmesine olanak tanır.
        services.TryAddScoped<IStatusCodeHandler, NotFoundStatusCodeHandler>();
        services.TryAddScoped<IStatusCodeHandler, UnauthorizedStatusCodeHandler>();
        services.TryAddScoped<IStatusCodeHandler, InternalServerErrorHandler>();
        services.TryAddScoped<IStatusCodeHandler, TooManyRequestsHandler>();

        // ÖNEMLİ: Middleware genellikle Singleton olarak kaydedilmez çünkü
        // kapsamlı (scoped) veya geçici (transient) servisleri (örneğin DbContext veya IStatusCodeHandler'lar)
        // enjekte etmesi gerekebilir. Ancak middleware'in kendisi DI ile oluşturulmaz,
        // UseMiddleware<T> metodu bunu bizim için yönetir.

        return services;
    }

    /// <summary>
    /// StatusCodeHandlingMiddleware'i ASP.NET Core request pipeline'ına ekler.
    /// Bu metot AddStatusCodeHandling'den sonra çağrılmalıdır.
    /// </summary>
    /// <param name="app">Uygulama builder.</param>
    /// <returns>Yapılandırmaya devam etmek için IApplicationBuilder.</returns>
    public static IApplicationBuilder UseStatusCodeHandling(this IApplicationBuilder app)
    {
        // Middleware'i pipeline'a ekle
        // UseMiddleware<T> çağrısı, T tipindeki middleware'i DI'dan çözümleyerek
        // gerekli bağımlılıkları (RequestDelegate, ILogger, IEnumerable<IStatusCodeHandler>) sağlar.
        app.UseMiddleware<StatusCodeHandlingMiddleware>();

        return app;
    }
}