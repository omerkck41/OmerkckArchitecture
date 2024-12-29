using Microsoft.AspNetCore.Authorization;

namespace Core.Application.Authorization.Behaviors;

// Behavior: Role-Based Authorization Handler
public class RoleAuthorizationBehavior : AuthorizationHandler<RolesRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RolesRequirement requirement)
    {
        if (context.User?.Identity == null || !context.User.Identity.IsAuthenticated)
        {
            return Task.CompletedTask;
        }

        var userRoles = context.User.FindAll("role").Select(r => r.Value).ToList();
        if (userRoles.Any(role => requirement.AllowedRoles.Contains(role)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

// Requirement Definition for Roles
public class RolesRequirement(IEnumerable<string> allowedRoles) : IAuthorizationRequirement
{
    public IEnumerable<string> AllowedRoles { get; } = allowedRoles;
}