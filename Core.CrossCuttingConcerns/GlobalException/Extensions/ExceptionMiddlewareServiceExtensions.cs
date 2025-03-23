using Core.CrossCuttingConcerns.GlobalException.Handlers;
using Core.CrossCuttingConcerns.GlobalException.Middlewares;
using Core.CrossCuttingConcerns.GlobalException.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Core.CrossCuttingConcerns.GlobalException.Extensions;

public static class ExceptionMiddlewareServiceExtensions
{
    /// <summary>
    /// GlobalExceptionMiddleware'i devreye alır.
    /// </summary>
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }

    /// <summary>
    /// Hata yakalama ve model validasyon hatalarını ortak formata dönüştürme servislerini ekler.
    /// </summary>
    public static IServiceCollection AddExceptionMiddlewareServices(this IServiceCollection services)
    {
        // 1) Controller’ları ekle ve model validasyon hatalarını 
        //    UnifiedApiErrorResponse formatına dönüştürmek için
        //    InvalidModelStateResponseFactory ayarını yap
        services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        // ModelState içindeki hataları topla
                        var errors = context.ModelState
                            .Where(ms => ms.Value.Errors.Count > 0)
                            .Select(ms => new ValidationExceptionModel
                            {
                                Property = ms.Key,
                                Errors = ms.Value.Errors.Select(e => e.ErrorMessage)
                            })
                            .ToList();

                        // Tek tip UnifiedApiErrorResponse oluştur
                        var errorResponse = new UnifiedApiErrorResponse
                        {
                            Success = false,
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "Validation error",
                            ErrorType = "ValidationException",
                            Detail = "Validation failed for one or more fields.",
                            AdditionalData = errors
                        };

                        // 400 (BadRequest) ile birlikte özel yanıt dön
                        return new BadRequestObjectResult(errorResponse);
                    };
                });

        // 2) Özel exception handler servislerini ekle
        services.AddSingleton<IExceptionHandlerFactory, ExceptionHandlerFactory>();

        // Somut tipleri DI konteynerına ekle
        services.AddSingleton<ValidationExceptionHandler>();
        services.AddSingleton<GlobalExceptionHandler>();

        // Somut tipleri IExceptionHandler arayüzü üzerinden erişilebilir hale getir
        services.AddSingleton<IExceptionHandler>(sp => sp.GetRequiredService<ValidationExceptionHandler>());
        services.AddSingleton<IExceptionHandler>(sp => sp.GetRequiredService<GlobalExceptionHandler>());

        return services;
    }
}