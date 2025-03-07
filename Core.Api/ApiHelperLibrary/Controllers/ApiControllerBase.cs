using Core.Api.ApiHelperLibrary.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Core.Api.ApiHelperLibrary.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult HandleResult<T>(ApiResponse<T> response)
    {
        return response.ToActionResult();
    }
}