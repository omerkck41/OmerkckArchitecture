using Kck.Exceptions.Attributes;

namespace Kck.Exceptions;

/// <summary>
/// Thrown when an operation is not valid for the object's current state.
/// Results in an HTTP 400 response via GlobalExceptionHandler.
/// </summary>
[HttpStatusCode(400)]
public class CustomInvalidOperationException : CustomException
{
    /// <summary>
    /// Initializes a new instance with the specified message.
    /// </summary>
    /// <param name="message">Human-readable description of the invalid operation.</param>
    public CustomInvalidOperationException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance with the specified message and inner exception.
    /// </summary>
    /// <param name="message">Human-readable description of the invalid operation.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public CustomInvalidOperationException(string message, Exception innerException)
        : base(message, innerException) { }
}
