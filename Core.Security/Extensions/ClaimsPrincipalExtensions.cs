using System.Security.Claims;

namespace Core.Security.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static List<string>? GetClaims(this ClaimsPrincipal claimsPrincipal, string claimType)
    {
        return claimsPrincipal?.FindAll(claimType)?.Select(c => c.Value).ToList();
    }

    public static string? GetEmail(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal?.GetClaims(ClaimTypes.Email)?.FirstOrDefault();
    }

    public static List<string>? GetRoles(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal?.GetClaims(ClaimTypes.Role);
    }

    public static int? GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        var userId = claimsPrincipal?.GetClaims(ClaimTypes.NameIdentifier)?.FirstOrDefault();
        return userId != null ? int.Parse(userId) : null;
    }
}