using Kck.Exceptions.Attributes;

namespace Kck.Exceptions;

/// <summary>
/// Thrown when a method argument has an invalid value.
/// Results in an HTTP 400 response via GlobalExceptionHandler, optionally including the offending parameter name.
/// </summary>
[HttpStatusCode(400)]
public class CustomArgumentException : CustomException
{
    /// <summary>
    /// Initializes a new instance with the specified message and optional parameter name.
    /// When <paramref name="paramName"/> is provided it is included in <see cref="CustomException.AdditionalData"/>.
    /// </summary>
    /// <param name="message">Human-readable description of the invalid argument.</param>
    /// <param name="paramName">Name of the offending parameter; <c>null</c> if not applicable.</param>
    public CustomArgumentException(string message, string? paramName = null)
        : base(message, null, paramName is not null ? new { ParamName = paramName } : null, null) { }

    /// <summary>
    /// Initializes a new instance with the specified message and inner exception.
    /// </summary>
    /// <param name="message">Human-readable description of the invalid argument.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public CustomArgumentException(string message, Exception innerException)
        : base(message, innerException) { }
}
