using Core.CrossCuttingConcerns.GlobalException.Attributes;

namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

[HttpStatusCode(StatusCodes.Status401Unauthorized)]
public class SecurityTokenException : CustomException
{
    public SecurityTokenException(string message)
        : base(message, explicitStatusCode: null, additionalData: null, innerException: null)
    {
    }

    public SecurityTokenException(string message, Exception innerException)
        : base(message, explicitStatusCode: null, additionalData: null, innerException: innerException)
    {
    }
}