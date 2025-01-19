using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application.Authorization.Behaviors;

public static class AuthorizationPipelineExtensions
{
    public static IServiceCollection AddAuthorizationPipeline(this IServiceCollection services)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        return services;
    }
}