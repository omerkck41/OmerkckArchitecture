﻿using Core.API.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Core.API.Extensions;

public static class ApiServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<Filters.ValidationFilter>();
        });

        return services;
    }

    public static IApplicationBuilder UseApiMiddlewares(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();

        return app;
    }
}