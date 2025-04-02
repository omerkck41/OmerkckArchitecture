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
        // 1. Önce kendi middleware'imiz
        app.UseMiddleware<GlobalExceptionMiddleware>();

        // 2. Sonra ProblemDetails
        app.UseProblemDetails();

        return app;
    }
    public static IServiceCollection AddAdvancedExceptionHandling(this IServiceCollection services)
    {
        // 1. Handler'ları kaydet (Transient olarak)
        services.AddTransient<GlobalExceptionHandler>();
        services.AddTransient<ValidationExceptionHandler>();
        services.AddTransient<IExceptionHandler, GlobalExceptionHandler>(); // Interface kaydı
        services.AddTransient<IExceptionHandler, ValidationExceptionHandler>(); // Interface kaydı

        // 2. Factory'i singleton olarak kaydet
        services.AddSingleton<IExceptionHandlerFactory, ExceptionHandlerFactory>();

        // 3. ProblemDetails konfigürasyonu
        services.AddProblemDetails(options =>
        {
            options.OnBeforeWriteDetails = (ctx, problem) =>
            {
                problem.Extensions.Add("requestId", ctx.TraceIdentifier);
                problem.Extensions.Add("timestamp", DateTime.UtcNow);
            };

            // Kendi handler'larımızın yakalaması için
            options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
        });

        // 4. Model validasyon hataları
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "Validation error",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = context.HttpContext.Request.Path,
                    Detail = "One or more validation errors occurred."
                };

                problemDetails.Extensions.Add("requestId", context.HttpContext.TraceIdentifier);
                problemDetails.Extensions.Add("timestamp", DateTime.UtcNow);

                return new BadRequestObjectResult(problemDetails);
            };
        });

        return services;
    }
}
