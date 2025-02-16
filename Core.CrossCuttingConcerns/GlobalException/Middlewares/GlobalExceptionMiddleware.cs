using Core.CrossCuttingConcerns.GlobalException.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Core.CrossCuttingConcerns.GlobalException.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;

    public GlobalExceptionMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
    {
        _next = next;
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            using var scope = _serviceProvider.CreateScope();
            var exceptionHandler = scope.ServiceProvider.GetRequiredService<IExceptionHandler>();
            await exceptionHandler.HandleExceptionAsync(context, ex);
        }
    }
}