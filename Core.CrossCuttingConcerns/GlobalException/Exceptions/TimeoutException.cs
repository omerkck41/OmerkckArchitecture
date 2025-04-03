using Core.CrossCuttingConcerns.GlobalException.Attributes;

namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

[HttpStatusCode(StatusCodes.Status408RequestTimeout)]
public class TimeoutException : CustomException
{
    public TimeoutException(string message = "Request timed out")
        : base(message, explicitStatusCode: null, additionalData: null, innerException: null)
    {
    }

    public TimeoutException(string message, Exception innerException)
        : base(message, explicitStatusCode: null, additionalData: null, innerException: innerException)
    {
    }
}