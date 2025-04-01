using Core.CrossCuttingConcerns.GlobalException.Attributes;
using Microsoft.AspNetCore.Http;

namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

[HttpStatusCode(StatusCodes.Status409Conflict)]
public class ConflictException : CustomException
{
    public ConflictException(string message = "Conflict occurred")
        : base(message, explicitStatusCode: null, additionalData: null, innerException: null)
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, explicitStatusCode: null, additionalData: null, innerException: innerException)
    {
    }
}