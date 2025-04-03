using Core.CrossCuttingConcerns.GlobalException.Attributes;

namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

[HttpStatusCode(StatusCodes.Status429TooManyRequests)]
public class TooManyRequestsException : CustomException
{
    public TooManyRequestsException(string message = "Too Many Requests")
        : base(message, explicitStatusCode: null, additionalData: null, innerException: null)
    {
    }

    public TooManyRequestsException(string message, Exception innerException)
        : base(message, explicitStatusCode: null, additionalData: null, innerException: innerException)
    {
    }
}