using Core.Application.Authorization.Behaviors;
using Core.Application.Authorization.Models;
using Core.Security.Extensions;
using System.Security.Claims;

namespace Core.Application.Authorization.Services;

public static class AuthorizationValidator
{
    public static void ValidateAuthorization(ClaimsPrincipal user, IEnumerable<string> requiredRoles, IDictionary<string, string> requiredClaims)
    {
        // Authentication kontrolü - 401
        if (user is null || !user.Identity?.IsAuthenticated == true)
        {
            throw new UnauthenticatedException();
        }

        // Authorization kontrolleri - 403
        if (requiredRoles.Any() && !HasRequiredRoles(user, requiredRoles))
        {
            throw new ForbiddenAccessException("Missing required roles");
        }

        if (requiredClaims?.Any() == true && !HasRequiredClaims(user, requiredClaims))
        {
            throw new ForbiddenAccessException("Missing required claims");
        }
    }

    private static bool HasRequiredRoles(ClaimsPrincipal user, IEnumerable<string> requiredRoles)
    {
        var userRoles = user.GetRoles() ?? Enumerable.Empty<string>();

        // Admin/Manager bypass yetkisi
        if (userRoles.Contains(GeneralOperationClaims.Admin) ||
            userRoles.Contains(GeneralOperationClaims.Manager))
        {
            return true;
        }

        return userRoles.Intersect(requiredRoles).Any();
    }

    private static bool HasRequiredClaims(ClaimsPrincipal user, IDictionary<string, string> requiredClaims)
    {
        // Admin/Manager bypass yetkisi
        if (user.IsInRole(GeneralOperationClaims.Admin) ||
            user.IsInRole(GeneralOperationClaims.Manager))
        {
            return true;
        }

        return requiredClaims.All(claim =>
            user.HasClaim(c => c.Type == claim.Key && c.Value == claim.Value));
    }
}