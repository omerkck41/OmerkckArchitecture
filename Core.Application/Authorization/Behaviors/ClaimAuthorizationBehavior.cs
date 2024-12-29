using Microsoft.AspNetCore.Authorization;

namespace Core.Application.Authorization.Behaviors;

// Behavior: Claim-Based Authorization Handler
public class ClaimAuthorizationBehavior : AuthorizationHandler<ClaimRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ClaimRequirement requirement)
    {
        if (context.User?.Identity == null || !context.User.Identity.IsAuthenticated)
        {
            return Task.CompletedTask;
        }

        var hasClaim = context.User.HasClaim(c => c.Type == requirement.ClaimType && c.Value == requirement.ClaimValue);
        if (hasClaim)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

}

// Requirement Definition for Claims
public class ClaimRequirement : IAuthorizationRequirement
{
    public string ClaimType { get; }
    public string ClaimValue { get; }

    public ClaimRequirement(string claimType, string claimValue)
    {
        ClaimType = claimType;
        ClaimValue = claimValue;
    }
}