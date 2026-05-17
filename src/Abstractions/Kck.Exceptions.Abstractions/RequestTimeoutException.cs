using Kck.Exceptions.Attributes;

namespace Kck.Exceptions;

/// <summary>
/// Thrown when the server did not receive a complete request within the allowed time.
/// Results in an HTTP 408 response via GlobalExceptionHandler.
/// </summary>
[HttpStatusCode(408)]
public class RequestTimeoutException : CustomException
{
    /// <summary>
    /// Initializes a new instance with the specified message.
    /// Defaults to "Request timed out" when no message is provided.
    /// </summary>
    /// <param name="message">Human-readable description of the timeout.</param>
    public RequestTimeoutException(string message = "Request timed out")
        : base(message) { }

    /// <summary>
    /// Initializes a new instance with the specified message and inner exception.
    /// </summary>
    /// <param name="message">Human-readable description of the timeout.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public RequestTimeoutException(string message, Exception innerException)
        : base(message, innerException) { }
}
