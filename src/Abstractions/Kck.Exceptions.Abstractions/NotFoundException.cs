using Kck.Exceptions.Attributes;

namespace Kck.Exceptions;

/// <summary>
/// Thrown when the requested resource does not exist or cannot be found.
/// Results in an HTTP 404 response via GlobalExceptionHandler.
/// </summary>
[HttpStatusCode(404)]
public class NotFoundException : CustomException
{
    /// <summary>
    /// Initializes a new instance with the specified message.
    /// </summary>
    /// <param name="message">Human-readable description of the missing resource.</param>
    public NotFoundException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance with the specified message and additional structured data.
    /// </summary>
    /// <param name="message">Human-readable description of the missing resource.</param>
    /// <param name="additionalData">Structured payload included in the error response.</param>
    public NotFoundException(string message, object additionalData)
        : base(message, null, additionalData, null) { }

    /// <summary>
    /// Initializes a new instance with the specified message and inner exception.
    /// </summary>
    /// <param name="message">Human-readable description of the missing resource.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public NotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
}
