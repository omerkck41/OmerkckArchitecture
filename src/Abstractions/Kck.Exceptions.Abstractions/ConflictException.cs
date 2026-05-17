using Kck.Exceptions.Attributes;

namespace Kck.Exceptions;

/// <summary>
/// Thrown when the request conflicts with the current state of the target resource (e.g., duplicate entry).
/// Results in an HTTP 409 response via GlobalExceptionHandler.
/// </summary>
[HttpStatusCode(409)]
public class ConflictException : CustomException
{
    /// <summary>
    /// Initializes a new instance with the specified message.
    /// Defaults to "Conflict occurred" when no message is provided.
    /// </summary>
    /// <param name="message">Human-readable description of the conflict.</param>
    public ConflictException(string message = "Conflict occurred")
        : base(message) { }

    /// <summary>
    /// Initializes a new instance with the specified message and inner exception.
    /// </summary>
    /// <param name="message">Human-readable description of the conflict.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public ConflictException(string message, Exception innerException)
        : base(message, innerException) { }
}
