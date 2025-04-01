using Core.CrossCuttingConcerns.GlobalException.Attributes;
using Microsoft.AspNetCore.Http;

namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

[HttpStatusCode(StatusCodes.Status401Unauthorized)]
public class UnauthorizedException : CustomException
{
    public UnauthorizedException(string message = "Authentication required")
        : base(message, explicitStatusCode: null, additionalData: null, innerException: null)
    {
    }

    public UnauthorizedException(string message, Exception innerException)
        : base(message, explicitStatusCode: null, additionalData: null, innerException: innerException)
    {
    }
}