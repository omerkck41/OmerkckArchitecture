using Core.CrossCuttingConcerns.GlobalException.Handlers;
using Microsoft.AspNetCore.Http;

namespace Core.CrossCuttingConcerns.GlobalException.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IExceptionHandlerFactory _handlerFactory;

    public GlobalExceptionMiddleware(RequestDelegate next, IExceptionHandlerFactory handlerFactory)
    {
        _next = next;
        _handlerFactory = handlerFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var handler = _handlerFactory.GetHandler(ex);
            await handler.HandleExceptionAsync(context, ex);
        }
    }
}