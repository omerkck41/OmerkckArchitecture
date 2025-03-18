using Core.Application.Authorization.Behaviors;
using Core.Application.Authorization.Models;
using Core.Security.Extensions;
using System.Security.Claims;

namespace Core.Application.Authorization.Services;

public static class AuthorizationValidator
{
    public static void ValidateAuthorization(ClaimsPrincipal user, IEnumerable<string> requiredRoles, IDictionary<string, string> requiredClaims)
    {
        if (user is null || !user.Identity?.IsAuthenticated == true)
        {
            throw new AuthorizationException("You are not authenticated...");
        }

        // Eğer requiredRoles tanımlı değilse (boşsa), rol kontrolünü atla.
        if (!requiredRoles.Any())
        {
            return;
        }

        ValidateRoles(user, requiredRoles);
        ValidateClaims(user, requiredClaims);
    }

    private static void ValidateRoles(ClaimsPrincipal user, IEnumerable<string> requiredRoles)
    {
        var userRoles = user.GetRoles() ?? Enumerable.Empty<string>();

        // Eğer kullanıcı Admin veya Manager rolüne sahipse, rol kontrolünü bypass ediyoruz.
        bool isAdminOrManager = userRoles.Contains(GeneralOperationClaims.Admin) || userRoles.Contains(GeneralOperationClaims.Manager);
        bool hasAnyRequiredRole = userRoles.Intersect(requiredRoles).Any();

        if (!isAdminOrManager && !hasAnyRequiredRole)
        {
            throw new AuthorizationException("You are not authorized for the required roles...");
        }
    }

    private static void ValidateClaims(ClaimsPrincipal user, IDictionary<string, string> requiredClaims)
    {
        // Eğer kullanıcı Admin veya Manager rolündeyse, claim kontrollerini atla.
        var userRoles = user.GetRoles() ?? Enumerable.Empty<string>();
        if (userRoles.Contains(GeneralOperationClaims.Admin) ||
            userRoles.Contains(GeneralOperationClaims.Manager))
        {
            return;
        }

        // Eğer requiredClaims null veya boş ise claim kontrolünü yapmadan çık.
        if (requiredClaims == null || !requiredClaims.Any())
        {
            return;
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