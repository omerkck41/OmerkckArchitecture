using Kck.Exceptions.Attributes;

namespace Kck.Exceptions;

/// <summary>
/// Thrown when the request is malformed or contains invalid data.
/// Results in an HTTP 400 response via GlobalExceptionHandler.
/// </summary>
[HttpStatusCode(400)]
public class BadRequestException : CustomException
{
    /// <summary>
    /// Initializes a new instance with the specified message.
    /// </summary>
    /// <param name="message">Human-readable description of the bad request.</param>
    public BadRequestException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance with the specified message and additional structured data.
    /// </summary>
    /// <param name="message">Human-readable description of the bad request.</param>
    /// <param name="additionalData">Structured payload included in the error response.</param>
    public BadRequestException(string message, object additionalData)
        : base(message, null, additionalData, null) { }

    /// <summary>
    /// Initializes a new instance with the specified message and inner exception.
    /// </summary>
    /// <param name="message">Human-readable description of the bad request.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public BadRequestException(string message, Exception innerException)
        : base(message, innerException) { }
}
