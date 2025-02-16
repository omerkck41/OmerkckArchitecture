using Core.CrossCuttingConcerns.GlobalException.Handlers;
using Core.CrossCuttingConcerns.GlobalException.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddScoped<IExceptionHandler, ValidationExceptionHandler>();
        return services;
    }
}