using Core.CrossCuttingConcerns.GlobalException.Handlers;
using Core.CrossCuttingConcerns.GlobalException.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Core.CrossCuttingConcerns.GlobalException.Extensions;

public static class ExceptionMiddlewareServiceExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }

    // Middleware için gerekli servisleri ekleyin
    public static IServiceCollection AddExceptionMiddlewareServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IExceptionHandlerFactory, ExceptionHandlerFactory>();

        services.TryAddSingleton<IExceptionHandler, ValidationExceptionHandler>();
        services.TryAddSingleton<IExceptionHandler, GlobalExceptionHandler>();
        return services;
    }
}