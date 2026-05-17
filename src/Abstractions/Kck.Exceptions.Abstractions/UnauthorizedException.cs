using Kck.Exceptions.Attributes;

namespace Kck.Exceptions;

/// <summary>
/// Thrown when the caller is not authenticated or credentials are missing.
/// Results in an HTTP 401 response via GlobalExceptionHandler.
/// </summary>
[HttpStatusCode(401)]
public class UnauthorizedException : CustomException
{
    /// <summary>
    /// Initializes a new instance with the specified message.
    /// Defaults to "Authentication required" when no message is provided.
    /// </summary>
    /// <param name="message">Human-readable description of the authentication failure.</param>
    public UnauthorizedException(string message = "Authentication required")
        : base(message) { }

    /// <summary>
    /// Initializes a new instance with the specified message and inner exception.
    /// </summary>
    /// <param name="message">Human-readable description of the authentication failure.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public UnauthorizedException(string message, Exception innerException)
        : base(message, innerException) { }
}
