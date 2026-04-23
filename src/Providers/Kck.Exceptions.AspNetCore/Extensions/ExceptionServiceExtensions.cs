using Kck.Exceptions.AspNetCore.Handlers;
using Kck.Exceptions.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kck.Exceptions.AspNetCore.Extensions;

/// <summary>
/// KCK exception handling DI kaydı ve middleware pipeline uzantıları.
/// </summary>
public static class ExceptionServiceExtensions
{
    /// <summary>
    /// KCK exception handler'larını ve ProblemDetails servislerini DI container'a kaydeder.
    /// </summary>
    public static IServiceCollection AddKckExceptionHandling(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.TryAddSingleton<GlobalExceptionHandler>();
        services.TryAddSingleton<ValidationExceptionHandler>();
        services.TryAddSingleton<ExceptionHandlerFactory>();
        return services;
    }

    /// <summary>
    /// KCK exception middleware'ini pipeline'a ekler.
    /// </summary>
    public static IApplicationBuilder UseKckExceptionHandling(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseStatusCodePages();
        return app;
    }
}
