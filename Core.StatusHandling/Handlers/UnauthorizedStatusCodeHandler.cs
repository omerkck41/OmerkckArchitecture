using Core.StatusHandling.Extensions;
using Core.StatusHandling.Interfaces;
using Core.StatusHandling.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Core.StatusHandling.Handlers;

public class UnauthorizedStatusCodeHandler : IStatusCodeHandler
{
    private readonly StatusCodeHandlingOptions _options;

    public UnauthorizedStatusCodeHandler(IOptions<StatusCodeHandlingOptions> options)
    {
        _options = options.Value;
    }

    // Bu handler 401 (Unauthorized) ve 403 (Forbidden) kodlarını işler
    public bool CanHandle(int statusCode) => statusCode == 401 || statusCode == 403;

    public async Task HandleAsync(HttpContext context)
    {
        string redirectPath = null;
        string notificationMessage = "You are not authorized for this operation.";
        string notificationType = "warning";

        if (context.Response.StatusCode == 401) // Giriş yapılmamış
        {
            notificationMessage = "Please log in.";
            notificationType = "info";
            // Yapılandırmadan 401 için yolu al
            _options.RedirectPaths.TryGetValue(401, out redirectPath);
            redirectPath ??= "/Login"; // Varsayılan login yolu
        }
        else if (context.Response.StatusCode == 403) // Yetki yetersiz
        {
            notificationMessage = "You are not authorized for this operation.";
            notificationType = "warning";
            // Yapılandırmadan 403 için yolu al
            _options.RedirectPaths.TryGetValue(403, out redirectPath);
            redirectPath ??= "/"; // Varsayılan olarak anasayfaya veya dashboard'a yönlendir
        }

        if (!string.IsNullOrEmpty(redirectPath))
        {
            context.Response.Redirect(redirectPath);
        }
        else
        {
            // Yönlendirme yoksa, varsayılan bir mesaj gösterilebilir
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync(notificationMessage); // Basit mesaj
        }


        // Bildirimleri etkinse, işaret bırak
        if (_options.EnableNotifications)
        {
            context.AppendNotification(notificationMessage, notificationType);
        }
    }
}