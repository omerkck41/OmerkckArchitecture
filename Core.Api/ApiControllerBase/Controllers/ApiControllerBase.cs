using Core.Api.ApiControllerBase.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Core.Api.ApiControllerBase.Controllers;

/// <summary>
/// Tüm API controller'lar için ortak özellikler ve yardımcı metodlar içerir.
/// </summary>
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// ApiResponse nesnesini IActionResult'a dönüştürür.
    /// Sadece başarılı işlemler için kullanılmalı, hata durumlarında exception fırlatılarak global middleware devreye girmelidir.
    /// </summary>
    protected IActionResult HandleResult<T>(ApiResponse<T> response)
    {
        return response.ToActionResult(HttpContext);
    }
}