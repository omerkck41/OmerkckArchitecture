using Core.Api.ApiControllerBase.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Core.Api.ApiControllerBase.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult HandleResult<T>(ApiResponse<T> response)
    {
        return response.ToActionResult();
    }
}