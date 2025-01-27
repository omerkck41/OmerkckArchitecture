﻿using Microsoft.AspNetCore.Mvc;

namespace Core.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult ApiResponse<T>(T data, string message = "Success")
    {
        return Ok(new { data, message });
    }

    protected IActionResult ApiError(string message, int statusCode = 400)
    {
        return StatusCode(statusCode, new { message });
    }

    protected IActionResult ApiValidationError(IEnumerable<string> errors)
    {
        return BadRequest(new { errors });
    }
}