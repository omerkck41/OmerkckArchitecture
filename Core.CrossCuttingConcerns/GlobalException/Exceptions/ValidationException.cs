using Core.CrossCuttingConcerns.GlobalException.Attributes;
using Core.CrossCuttingConcerns.GlobalException.Models;
using Microsoft.AspNetCore.Http;

namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

[HttpStatusCode(StatusCodes.Status400BadRequest)]
public class ValidationException : CustomException
{
    public IEnumerable<ValidationExceptionModel> Errors { get; }

    public ValidationException(IEnumerable<ValidationExceptionModel> errors)
        : base(BuildErrorMessage(errors), explicitStatusCode: null, additionalData: errors, innerException: null)
    {
        Errors = errors;
    }

    public ValidationException(string message, IEnumerable<ValidationExceptionModel> errors)
        : base(message, explicitStatusCode: null, additionalData: errors, innerException: null)
    {
        Errors = errors;
    }

    public ValidationException(string message, Exception innerException)
        : base(message, explicitStatusCode: null, additionalData: null, innerException: innerException)
    {
        Errors = Array.Empty<ValidationExceptionModel>();
    }

    private static string BuildErrorMessage(IEnumerable<ValidationExceptionModel> errors)
    {
        return $"Validation failed: {string.Join(", ", errors.Select(e => $"{e.Property}: {string.Join(", ", e.Errors)}"))}";
    }
}