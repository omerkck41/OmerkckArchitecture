using Core.CrossCuttingConcerns.GlobalException.Exceptions;

namespace Core.Application.Authorization.Behaviors;

public abstract class AuthorizationException : CustomException
{
    protected AuthorizationException(string message, int explicitStatusCode)
        : base(message, explicitStatusCode, additionalData: null, innerException: null)
    {
    }
}