using Kck.Exceptions.AspNetCore.Handlers;
using Kck.Exceptions.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kck.Exceptions.AspNetCore.Extensions;

/// <summary>
/// KCK exception handling DI registration and middleware pipeline extensions.
/// </summary>
public static class ExceptionServiceExtensions
{
    /// <summary>
    /// Registers the KCK exception handlers and ProblemDetails services in the DI container.
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
    /// Adds the KCK exception middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseKckExceptionHandling(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseStatusCodePages();
        return app;
    }
}
