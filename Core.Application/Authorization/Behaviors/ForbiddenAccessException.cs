using Microsoft.AspNetCore.Http;

namespace Core.Application.Authorization.Behaviors;

public class ForbiddenAccessException : AuthorizationException
{
    public ForbiddenAccessException(string message = "Insufficient permissions")
        : base(message, StatusCodes.Status403Forbidden)
    {
    }
}