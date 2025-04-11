using Core.CrossCuttingConcerns.GlobalException.Handlers;
using Microsoft.Extensions.Options;

namespace Core.CrossCuttingConcerns.GlobalException.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IExceptionHandlerFactory _handlerFactory;
    private readonly GlobalExceptionOptions _options;

    public GlobalExceptionMiddleware(RequestDelegate next, IExceptionHandlerFactory handlerFactory, IOptions<GlobalExceptionOptions> options)
    {
        _next = next;
        _handlerFactory = handlerFactory;
        _options = options.Value;
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

            var isHtml = context.Request.Headers.Accept.ToString().Contains("text/html");

            if (_options.OnExceptionCompletedAsync is not null)
            {
                await _options.OnExceptionCompletedAsync.Invoke(context, ex, isHtml);
                return;
            }

            await handler.HandleExceptionAsync(context, ex);
        }
    }
}