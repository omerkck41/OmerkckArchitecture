using Core.CrossCuttingConcerns.GlobalException.Handlers;
using Core.CrossCuttingConcerns.GlobalException.Middlewares;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Core.CrossCuttingConcerns.GlobalException.Extensions;

public static class ExceptionMiddlewareServiceExtensions
{
    public static IApplicationBuilder UseAdvancedExceptionHandling(this IApplicationBuilder app)
    {
        // Problem Details middleware'ini etkinleştir
        app.UseProblemDetails();

        // Özel exception middleware'imiz
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
    public static IServiceCollection AddAdvancedExceptionHandling(this IServiceCollection services)
    {
        // Problem Details yapılandırması
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = ctx =>
            {
                ctx.ProblemDetails.Extensions.Add("requestId", ctx.HttpContext.TraceIdentifier);
            };
        });

        // Exception handler'larını kaydet
        services.AddSingleton<IExceptionHandler, GlobalExceptionHandler>();
        services.AddSingleton<IExceptionHandler, ValidationExceptionHandler>();
        services.AddSingleton<IExceptionHandlerFactory, ExceptionHandlerFactory>();

        // Model validasyon hataları için
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "Validation error",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = context.HttpContext.Request.Path
                };

                return new BadRequestObjectResult(problemDetails);
            };
        });

        return services;
    }
}