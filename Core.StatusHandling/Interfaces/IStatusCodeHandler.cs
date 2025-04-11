using Microsoft.AspNetCore.Http;

namespace Core.StatusHandling.Interfaces;

/// <summary>
/// Belirli HTTP durum kodlarını işlemek için temel arayüz.
/// </summary>
public interface IStatusCodeHandler
{
    /// <summary>
    /// Bu işleyicinin verilen durum kodunu işleyip işleyemeyeceğini belirler.
    /// </summary>
    /// <param name="statusCode">İşlenecek HTTP durum kodu.</param>
    /// <returns>İşleyici bu kodu işleyebiliyorsa true, aksi takdirde false.</returns>
    bool CanHandle(int statusCode);

    /// <summary>
    /// Belirlenen durum kodu için özel işlemi gerçekleştirir.
    /// </summary>
    /// <param name="context">Mevcut HTTP bağlamı.</param>
    /// <returns>İşlemin tamamlandığını belirten bir Task.</returns>
    Task HandleAsync(HttpContext context);
}