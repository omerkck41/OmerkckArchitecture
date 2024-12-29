using Core.Application.Authorization.Behaviors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application.Authorization.Services;

// Service: Authorization Policy Registration
public static class AuthorizationPolicyService
{
    public static void AddRolePolicies(IServiceCollection services)
    {
        services.AddAuthorizationCore(options =>
        {
            options.AddPolicy("AdminPolicy", policy =>
                policy.Requirements.Add(new RolesRequirement(["Admin"])));
        });

        services.AddSingleton<IAuthorizationHandler, RoleAuthorizationBehavior>();
    }

    public static void AddClaimPolicies(IServiceCollection services)
    {
        services.AddAuthorizationCore(options =>
        {
            options.AddPolicy("CustomClaimPolicy", policy =>
                policy.Requirements.Add(new ClaimRequirement("CustomClaim", "Allowed")));
        });

        services.AddSingleton<IAuthorizationHandler, ClaimAuthorizationBehavior>();
    }
}