using System.Security.Claims;

namespace Core.Application.Authorization.Services;

public class AuthorizationService : IAuthorizationService
{
    public void Authorize(ClaimsPrincipal user, string[] requiredRoles, Dictionary<string, string> requiredClaims)
    {
        AuthorizationValidator.ValidateAuthorization(user, requiredRoles, requiredClaims);
    }
}