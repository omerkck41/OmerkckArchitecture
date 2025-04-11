using Core.StatusHandling.Extensions;
using Core.StatusHandling.Interfaces;
using Core.StatusHandling.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Core.StatusHandling.Handlers;

public class NotFoundStatusCodeHandler : IStatusCodeHandler
{
    private readonly StatusCodeHandlingOptions _options;

    // Yapılandırma seçeneklerini almak için IOptions kullanıyoruz
    public NotFoundStatusCodeHandler(IOptions<StatusCodeHandlingOptions> options)
    {
        _options = options.Value; // Seçeneklerin değerini alıyoruz
    }

    public bool CanHandle(int statusCode) => statusCode == 404;

    public async Task HandleAsync(HttpContext context)
    {
        // Yapılandırmada 404 için özel bir yol tanımlanmış mı kontrol et
        if (_options.RedirectPaths.TryGetValue(404, out var redirectPath) && !string.IsNullOrEmpty(redirectPath))
        {
            context.Response.Redirect(redirectPath);
        }
        else
        {
            // Varsayılan davranış: Basit bir mesaj yaz veya ana sayfaya yönlendir (isteğe bağlı)
            // context.Response.Redirect("/"); // Ana sayfaya yönlendirme örneği
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Source not found (404).");
        }

        // Bildirimleri etkinse, bir işaret bırak (örneğin cookie veya header)
        // Gerçek Toast bildirimi UI katmanında bu işarete göre gösterilir.
        if (_options.EnableNotifications)
        {
            // Ortak extension kullanılarak bildirim ekleniyor
            context.AppendNotification("The page you are looking for was not found.", "info");
        }
    }
}