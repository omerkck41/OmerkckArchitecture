using Microsoft.AspNetCore.Http;

namespace Core.Application.Authorization.Behaviors;

public class UnauthenticatedException : AuthorizationException
{
    public UnauthenticatedException(string message = "Authentication required")
        : base(message, StatusCodes.Status401Unauthorized)
    {
    }
}