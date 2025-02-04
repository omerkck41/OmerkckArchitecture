using Core.CrossCuttingConcerns.GlobalException.Handlers;
using Microsoft.AspNetCore.Http;

namespace Core.CrossCuttingConcerns.GlobalException.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IExceptionHandler _exceptionHandler;

    public GlobalExceptionMiddleware(RequestDelegate next, IExceptionHandler exceptionHandler)
    {
        _next = next;
        _exceptionHandler = exceptionHandler;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (System.Exception ex)
        {
            await _exceptionHandler.HandleExceptionAsync(context, ex);
        }
    }
}