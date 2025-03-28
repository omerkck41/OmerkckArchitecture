﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Core.Application.Authorization.Middleware;

public class HttpAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public HttpAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Endpoint metadata bilgisini alıyoruz
        var endpoint = context.GetEndpoint();
        if (endpoint != null && endpoint.Metadata.Any(m => m is IAllowAnonymous))
        {
            // Eğer endpoint [AllowAnonymous] içeriyorsa, kontrolü atla
            await _next(context);
            return;
        }

        if (context.User == null || !context.User.Identity!.IsAuthenticated)
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("Authentication is required.");
            return;
        }

        await _next(context);
    }
}