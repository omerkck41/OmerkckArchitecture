using Core.StatusHandling.Extensions;
using Core.StatusHandling.Interfaces;
using Core.StatusHandling.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Core.StatusHandling.Handlers;

public class InternalServerErrorHandler : IStatusCodeHandler
{
    private readonly StatusCodeHandlingOptions _options;
    private readonly ILogger<InternalServerErrorHandler> _logger;

    public InternalServerErrorHandler(IOptions<StatusCodeHandlingOptions> options, ILogger<InternalServerErrorHandler> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public bool CanHandle(int statusCode) => statusCode == 500;

    public async Task HandleAsync(HttpContext context)
    {
        _logger.LogError("500 Internal Server Error occurred. URL: {Url}", context.Request.Path);

        string redirectPath = null;
        // Yapılandırmada 500 için yönlendirme yolu tanımlı ise onu kullanır
        if (_options.RedirectPaths.TryGetValue(500, out redirectPath) && !string.IsNullOrEmpty(redirectPath))
        {
            context.Response.Redirect(redirectPath);
        }
        else
        {
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Server error (500). Please try again later.");
        }

        if (_options.EnableNotifications)
        {
            // Kullanıcıya yönelik bildirim ekleniyor (örneğin, toast mesajı için cookie ekleme)
            context.AppendNotification("There was an error on the server. Please try again.", "error");
        }
    }
}