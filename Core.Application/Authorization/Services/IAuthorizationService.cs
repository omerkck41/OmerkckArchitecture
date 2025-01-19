using System.Security.Claims;

namespace Core.Application.Authorization.Services;

public interface IAuthorizationService
{
    void Authorize(ClaimsPrincipal user, string[] requiredRoles, Dictionary<string, string> requiredClaims);
}