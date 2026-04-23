using Kck.Exceptions.AspNetCore.Models;
using Microsoft.AspNetCore.Http;

namespace Kck.Exceptions.AspNetCore.Handlers;

/// <summary>
/// KCK exception handler arayüzü.
/// Her handler, belirli exception tiplerini işleyebilir.
/// </summary>
public interface IKckExceptionHandler
{
    /// <summary>
    /// Bu handler'ın verilen exception'ı işleyip işleyemeyeceğini belirler.
    /// </summary>
    bool CanHandle(Exception exception);

    /// <summary>
    /// Exception'ı işler ve RFC 7807 uyumlu hata yanıtı oluşturur.
    /// </summary>
    UnifiedApiErrorResponse Handle(Exception exception, HttpContext context);
}
