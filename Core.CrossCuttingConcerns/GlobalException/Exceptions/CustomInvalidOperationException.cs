using Core.CrossCuttingConcerns.GlobalException.Attributes;

namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

[HttpStatusCode(StatusCodes.Status400BadRequest)]
public class CustomInvalidOperationException : CustomException
{
    public CustomInvalidOperationException(string message)
        : base(message, explicitStatusCode: null, additionalData: null, innerException: null)
    {
    }

    public CustomInvalidOperationException(string message, Exception innerException)
        : base(message, explicitStatusCode: null, additionalData: null, innerException: innerException)
    {
    }
}