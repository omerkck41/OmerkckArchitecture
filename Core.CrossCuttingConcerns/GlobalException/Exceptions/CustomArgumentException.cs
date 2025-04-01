using Core.CrossCuttingConcerns.GlobalException.Attributes;
using Microsoft.AspNetCore.Http;

namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

[HttpStatusCode(StatusCodes.Status400BadRequest)]
public class CustomArgumentException : CustomException
{
    public CustomArgumentException(string message, string? paramName = null)
        : base(message, explicitStatusCode: null, additionalData: new { ParamName = paramName }, innerException: null)
    {
    }

    public CustomArgumentException(string message, Exception innerException)
        : base(message, explicitStatusCode: null, additionalData: null, innerException: innerException)
    {
    }
}