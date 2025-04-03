using Core.CrossCuttingConcerns.GlobalException.Attributes;

namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

[HttpStatusCode(StatusCodes.Status403Forbidden)]
public class ForbiddenException : CustomException
{
    public ForbiddenException(string message = "Insufficient permissions")
        : base(message, explicitStatusCode: null, additionalData: null, innerException: null)
    {
    }

    public ForbiddenException(string message, Exception innerException)
        : base(message, explicitStatusCode: null, additionalData: null, innerException: innerException)
    {
    }
}