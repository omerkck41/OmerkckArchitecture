using Kck.Exceptions.Attributes;
using Kck.Exceptions.Models;

namespace Kck.Exceptions;

/// <summary>
/// Thrown when one or more input validation rules fail.
/// Results in an HTTP 400 response via GlobalExceptionHandler with a structured list of field-level errors.
/// </summary>
[HttpStatusCode(400)]
public class ValidationException : CustomException
{
    /// <summary>
    /// The collection of individual field validation failures included in the error response.
    /// </summary>
    public IEnumerable<ValidationExceptionModel> Errors { get; }

    /// <summary>
    /// Initializes a new instance, building the error message automatically from <paramref name="errors"/>.
    /// </summary>
    /// <param name="errors">The validation errors to include in the response.</param>
    public ValidationException(IEnumerable<ValidationExceptionModel> errors)
        : base(BuildErrorMessage(errors), null, errors, null)
    {
        Errors = errors;
    }

    /// <summary>
    /// Initializes a new instance with a custom message and a collection of validation errors.
    /// </summary>
    /// <param name="message">Human-readable summary of the validation failure.</param>
    /// <param name="errors">The validation errors to include in the response.</param>
    public ValidationException(string message, IEnumerable<ValidationExceptionModel> errors)
        : base(message, null, errors, null)
    {
        Errors = errors;
    }

    /// <summary>
    /// Initializes a new instance with the specified message and inner exception.
    /// <see cref="Errors"/> will be empty in this case.
    /// </summary>
    /// <param name="message">Human-readable description of the validation failure.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        Errors = [];
    }

    private static string BuildErrorMessage(IEnumerable<ValidationExceptionModel> errors)
        => $"Validation failed: {string.Join(", ", errors.Select(e => $"{e.Property}: {string.Join(", ", e.Errors)}"))}";
}
