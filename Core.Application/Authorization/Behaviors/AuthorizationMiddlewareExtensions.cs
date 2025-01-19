using Microsoft.AspNetCore.Builder;

namespace Core.Application.Authorization.Behaviors;

public static class AuthorizationMiddlewareExtensions
{
    public static IApplicationBuilder UseHttpAuthorizationMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<HttpAuthorizationMiddleware>();
    }
}