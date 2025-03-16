using Core.Application.Authorization.Behaviors;
using Core.Application.Authorization.Models;
using Core.Security.Extensions;
using System.Security.Claims;

namespace Core.Application.Authorization.Services;

public static class AuthorizationValidator
{
    public static void ValidateAuthorization(ClaimsPrincipal user, IEnumerable<string> requiredRoles, IDictionary<string, string> requiredClaims)
    {
        if (user is not { Identity: { IsAuthenticated: true } })
        {
            throw new AuthorizationException("You are not authenticated.");
        }

        if (requiredRoles is null)
            throw new ArgumentNullException(nameof(requiredRoles));

        if (requiredClaims is null)
            throw new ArgumentNullException(nameof(requiredClaims));

        var userRoles = user.GetRoles() ?? Enumerable.Empty<string>();

        // Eğer kullanıcı Admin veya Manager rolüne sahipse, zorunlu roller kontrolünü bypass ediyoruz.
        if (!userRoles.Contains(GeneralOperationClaims.Admin) &&
            !userRoles.Contains(GeneralOperationClaims.Manager) &&
            !userRoles.Any(role => requiredRoles.Contains(role)))
        {
            throw new AuthorizationException("You are not authorized.");
        }

        foreach (var claim in requiredClaims)
        {
            if (!user.HasClaim(c => c.Type == claim.Key && c.Value == claim.Value))
            {
                throw new AuthorizationException($"Missing required claim: {claim.Key} - {claim.Value}");
            }
        }
    }
}