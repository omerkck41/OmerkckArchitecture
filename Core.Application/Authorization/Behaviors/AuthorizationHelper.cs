using Core.Security.Extensions;
using System.Security.Claims;

namespace Core.Application.Authorization.Behaviors;

public static class AuthorizationHelper
{
    public static bool IsUserInRole(this ClaimsPrincipal user, string role)
    {
        return user.GetRoles().Contains(role);
    }

    public static bool HasRequiredClaim(this ClaimsPrincipal user, string claimType, string claimValue)
    {
        return user.HasClaim(c => c.Type == claimType && c.Value == claimValue);
    }
}