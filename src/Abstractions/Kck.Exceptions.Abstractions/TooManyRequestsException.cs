using Kck.Exceptions.Attributes;

namespace Kck.Exceptions;

/// <summary>
/// Thrown when the caller has exceeded the allowed request rate limit.
/// Results in an HTTP 429 response via GlobalExceptionHandler.
/// </summary>
[HttpStatusCode(429)]
public class TooManyRequestsException : CustomException
{
    /// <summary>
    /// Initializes a new instance with the specified message.
    /// Defaults to "Too many requests" when no message is provided.
    /// </summary>
    /// <param name="message">Human-readable description of the rate limit violation.</param>
    public TooManyRequestsException(string message = "Too many requests")
        : base(message) { }

    /// <summary>
    /// Initializes a new instance with the specified message and inner exception.
    /// </summary>
    /// <param name="message">Human-readable description of the rate limit violation.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public TooManyRequestsException(string message, Exception innerException)
        : base(message, innerException) { }
}
