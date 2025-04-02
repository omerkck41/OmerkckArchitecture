﻿using Core.CrossCuttingConcerns.GlobalException.Handlers;
using Core.CrossCuttingConcerns.GlobalException.Middlewares;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ProblemDetailsOptions = Hellang.Middleware.ProblemDetails.ProblemDetailsOptions;

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

    public static IServiceCollection AddAdvancedExceptionHandling(this IServiceCollection services, Assembly[]? handlerAssemblies = null)
    {
        // 1. Temel handler'ları kaydet (Transient olarak)
        services.AddTransient<GlobalExceptionHandler>();
        services.AddTransient<ValidationExceptionHandler>();

        // 2. Factory'i singleton olarak kaydet
        services.AddSingleton<IExceptionHandlerFactory, ExceptionHandlerFactory>();

        // 3. Assembly'lerdeki tüm handler'ları otomatik kaydet
        RegisterHandlersFromAssemblies(handlerAssemblies);

        // 4. ProblemDetails konfigürasyonu
        services.AddProblemDetails(ConfigureProblemDetails);

        // 5. Model validasyon hataları
        services.Configure<ApiBehaviorOptions>(ConfigureApiBehavior);

        return services;
    }

    private static void RegisterHandlersFromAssemblies(Assembly[]? assemblies)
    {
        // Varsayılan olarak mevcut assembly'i kullan
        var targetAssemblies = assemblies ?? new[] { Assembly.GetExecutingAssembly() };

        foreach (var assembly in targetAssemblies)
        {
            ExceptionHandlerFactory.RegisterHandlersFromAssembly(assembly);
        }
    }

    private static void ConfigureProblemDetails(ProblemDetailsOptions options)
    {
        options.OnBeforeWriteDetails = (ctx, problem) =>
        {
            problem.Extensions.Add("requestId", ctx.TraceIdentifier);
            problem.Extensions.Add("timestamp", DateTime.UtcNow);
        };

        options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
    }

    private static void ConfigureApiBehavior(ApiBehaviorOptions options)
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
    }
}
