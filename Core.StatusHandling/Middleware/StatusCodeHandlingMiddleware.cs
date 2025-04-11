using Core.StatusHandling.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Core.StatusHandling.Middleware;

public class StatusCodeHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IEnumerable<IStatusCodeHandler> _handlers; // Tüm kayıtlı handler'lar
    private readonly ILogger<StatusCodeHandlingMiddleware> _logger; // Loglama

    public StatusCodeHandlingMiddleware(
        RequestDelegate next,
        IEnumerable<IStatusCodeHandler> handlers, // DI ile tüm handler'lar buraya gelir
        ILogger<StatusCodeHandlingMiddleware> logger)
    {
        _next = next;
        _handlers = handlers;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Önce sonraki middleware'i çağırıyoruz ki response status kodu belirlensin
        await _next(context);

        // Yanıt zaten başladıysa (örneğin dosya gönderimi) veya başarılıysa (2xx) veya yönlendirmeyse (3xx) dokunmuyoruz
        // Ayrıca WebSocket isteklerini de genellikle elle handle etmek isteriz.
        if (context.Response.HasStarted ||
            (context.Response.StatusCode >= 200 && context.Response.StatusCode <= 399) ||
            context.WebSockets.IsWebSocketRequest)
        {
            return;
        }

        // Durum kodunu al
        var statusCode = context.Response.StatusCode;

        // Bu durum kodunu işleyebilecek ilk handler'ı bul
        var handler = _handlers.FirstOrDefault(h => h.CanHandle(statusCode));

        if (handler != null)
        {
            _logger.LogDebug("Handler {HandlerName} for HTTP Status Code {StatusCode} has been found and will be executed.", statusCode, handler.GetType().Name);

            // Handler'ın response'u değiştirmesine izin vermek için
            // mevcut response body'yi temizleyebiliriz (isteğe bağlı, duruma göre karar verilir)
            // context.Response.Clear(); // Eğer handler tamamen yeni bir response üretecekse

            // Handler'ı çalıştır
            await handler.HandleAsync(context);

            // ÖNEMLİ NOT: Eğer handler bir yönlendirme (Redirect) yapıyorsa,
            // ASP.NET Core pipeline'ı genellikle burada kesilir ve sonraki adımlar çalışmaz.
            // Eğer handler sadece response body'yi değiştiriyorsa pipeline devam eder.
        }
        else
        {
            _logger.LogDebug("No suitable handler found for HTTP Status Code {StatusCode}.", statusCode);
            // Opsiyonel: Handler bulunamazsa varsayılan bir işlem yapılabilir.
            // Örneğin, UseStatusCodePages middleware'ine devredilebilir veya basit bir mesaj yazılabilir.
            // Dikkat: Eğer response boşsa ve bir handler bulunamazsa, istemci boş bir yanıt alabilir.
        }
    }
}