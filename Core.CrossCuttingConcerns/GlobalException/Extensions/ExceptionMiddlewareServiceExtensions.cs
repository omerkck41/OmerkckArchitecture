using Core.CrossCuttingConcerns.GlobalException.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Core.CrossCuttingConcerns.GlobalException.Extensions;

public static class ExceptionMiddlewareServiceExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}