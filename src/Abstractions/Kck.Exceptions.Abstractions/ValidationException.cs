using Kck.Exceptions.Attributes;
using Kck.Exceptions.Models;

namespace Kck.Exceptions;

[HttpStatusCode(400)]
public class ValidationException : CustomException
{
    public IEnumerable<ValidationExceptionModel> Errors { get; }

    public ValidationException(IEnumerable<ValidationExceptionModel> errors)
        : base(BuildErrorMessage(errors), null, errors, null)
    {
        Errors = errors;
    }

    public ValidationException(string message, IEnumerable<ValidationExceptionModel> errors)
        : base(message, null, errors, null)
    {
        Errors = errors;
    }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        Errors = [];
    }

    private static string BuildErrorMessage(IEnumerable<ValidationExceptionModel> errors)
        => $"Validation failed: {string.Join(", ", errors.Select(e => $"{e.Property}: {string.Join(", ", e.Errors)}"))}";
}
