using Microsoft.AspNetCore.Http;

namespace Core.Application.Authorization.Behaviors;

public class UnAuthenticatedException : AuthorizationException
{
    public UnAuthenticatedException(string message = "Authentication required")
        : base(message, StatusCodes.Status401Unauthorized)
    {
    }
}