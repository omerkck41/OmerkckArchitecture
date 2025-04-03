using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Core.CrossCuttingConcerns.GlobalException.Handlers;
using Core.CrossCuttingConcerns.GlobalException.Middlewares;
using Core.CrossCuttingConcerns.GlobalException.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ProblemDetailsOptions = Hellang.Middleware.ProblemDetails.ProblemDetailsOptions;

namespace Core.CrossCuttingConcerns.GlobalException.Extensions;

public static class ExceptionMiddlewareServiceExtensions
{
    public static IApplicationBuilder UseAdvancedExceptionHandling(this IApplicationBuilder app)
    {
        // 1. Önce ProblemDetails (sadece formatlama yapsın)
        app.UseProblemDetails();

        // 2. Sonra kendi middleware'imiz (exception'ları yakalasın)
        return app.UseMiddleware<GlobalExceptionMiddleware>();
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

        services.AddLocalization(options => options.ResourcesPath = "Resources");

        return services;
    }

    private static void RegisterHandlersFromAssemblies(Assembly[]? assemblies)
    {
        // Varsayılan olarak mevcut assembly'i kullan
        var targetAssemblies = assemblies ?? [Assembly.GetExecutingAssembly()];

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

            // OpenAPI dokümantasyonu için ek meta veri
            problem.Extensions.Add("x-custom-error-code", problem.Status);
        };

        options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
    }

    private static void ConfigureApiBehavior(ApiBehaviorOptions options)
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errorId = Guid.NewGuid().ToString();

            // ModelState'deki hataları, ValidationExceptionModel listesine dönüştürüyoruz.
            var errorModels = context.ModelState
                .Where(ms => ms.Value.Errors.Any())
                .Select(ms => new ValidationExceptionModel
                {
                    Property = ms.Key,
                    Errors = ms.Value.Errors.Select(e => e.ErrorMessage)
                })
                .ToList();

            // ValidationException oluşturularak, tüm validasyon hataları kapsanıyor.
            var validationException = new ValidationException(errorModels);

            // UnifiedApiErrorResponse'u, tanımlı yöntemle oluşturuyoruz.
            var unifiedResponse = UnifiedApiErrorResponse.FromValidationException(
                validationException,
                errorId,
                "Validation failed for one or more fields."
            );

            // İsteğin path bilgisini instance olarak ekliyoruz.
            unifiedResponse = unifiedResponse with { Instance = context.HttpContext.Request.Path };

            return new BadRequestObjectResult(unifiedResponse);
        };
    }

}
