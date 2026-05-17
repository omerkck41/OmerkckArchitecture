using Kck.Exceptions.Attributes;

namespace Kck.Exceptions;

/// <summary>
/// Thrown when a security token is invalid, expired, or cannot be validated.
/// Results in an HTTP 401 response via GlobalExceptionHandler.
/// </summary>
[HttpStatusCode(401)]
public class SecurityTokenException : CustomException
{
    /// <summary>
    /// Initializes a new instance with the specified message.
    /// </summary>
    /// <param name="message">Human-readable description of the token validation failure.</param>
    public SecurityTokenException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance with the specified message and inner exception.
    /// </summary>
    /// <param name="message">Human-readable description of the token validation failure.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public SecurityTokenException(string message, Exception innerException)
        : base(message, innerException) { }
}
