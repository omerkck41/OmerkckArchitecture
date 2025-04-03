using Core.CrossCuttingConcerns.GlobalException.Attributes;

namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

[HttpStatusCode(StatusCodes.Status404NotFound)]
public class NotFoundException : CustomException
{
    public NotFoundException(string message)
        : base(message, explicitStatusCode: null, additionalData: null, innerException: null)
    {
    }

    public NotFoundException(string message, object additionalData)
        : base(message, explicitStatusCode: null, additionalData: additionalData, innerException: null)
    {
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, explicitStatusCode: null, additionalData: null, innerException: innerException)
    {
    }
}