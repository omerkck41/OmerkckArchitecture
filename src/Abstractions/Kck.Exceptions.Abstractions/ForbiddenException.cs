using Kck.Exceptions.Attributes;

namespace Kck.Exceptions;

/// <summary>
/// Thrown when an authenticated caller lacks the necessary permissions for the requested operation.
/// Results in an HTTP 403 response via GlobalExceptionHandler.
/// </summary>
[HttpStatusCode(403)]
public class ForbiddenException : CustomException
{
    /// <summary>
    /// Initializes a new instance with the specified message.
    /// Defaults to "Insufficient permissions" when no message is provided.
    /// </summary>
    /// <param name="message">Human-readable description of the authorization failure.</param>
    public ForbiddenException(string message = "Insufficient permissions")
        : base(message) { }

    /// <summary>
    /// Initializes a new instance with the specified message and inner exception.
    /// </summary>
    /// <param name="message">Human-readable description of the authorization failure.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public ForbiddenException(string message, Exception innerException)
        : base(message, innerException) { }
}
