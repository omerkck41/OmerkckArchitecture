using Core.Application.Authorization.Behaviors;
using Core.Security.Extensions;
using System.Security.Claims;

namespace Core.Application.Authorization.Services;

public class AuthorizationService : IAuthorizationService
{
    public void Authorize(ClaimsPrincipal user, string[] requiredRoles, Dictionary<string, string> requiredClaims)
    {
        if (user == null || !user.Identity.IsAuthenticated)
        {
            throw new AuthorizationException("You are not authenticated.");
        }

        // Role-based authorization
        var userRoles = user.GetRoles();
        if (!userRoles.Any(role => requiredRoles.Contains(role) || role == GeneralOperationClaims.Admin))
        {
            throw new AuthorizationException("You are not authorized.");
        }

        // Claim-based authorization
        foreach (var claim in requiredClaims)
        {
            if (!user.HasClaim(c => c.Type == claim.Key && c.Value == claim.Value))
            {
                throw new AuthorizationException($"Missing required claim: {claim.Key} - {claim.Value}");
            }
        }
    }
}