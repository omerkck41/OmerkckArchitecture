using Core.CrossCuttingConcerns.GlobalException.Attributes;

namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

[HttpStatusCode(StatusCodes.Status400BadRequest)]
public class BadRequestException : CustomException
{
    public BadRequestException(string message)
        : base(message, explicitStatusCode: null, additionalData: null, innerException: null)
    {
    }

    public BadRequestException(string message, object additionalData)
        : base(message, explicitStatusCode: null, additionalData: additionalData, innerException: null)
    {
    }

    public BadRequestException(string message, Exception innerException)
        : base(message, explicitStatusCode: null, additionalData: null, innerException: innerException)
    {
    }
}