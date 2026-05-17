namespace Kck.Exceptions;

/// <summary>
/// Base class for all Kck domain exceptions.
/// Carries <see cref="ErrorType"/>, an optional <see cref="ExplicitStatusCode"/>, and <see cref="AdditionalData"/> for structured error responses.
/// </summary>
public abstract class CustomException : Exception
{
    /// <summary>
    /// Overrides <see cref="Kck.Exceptions.Attributes.HttpStatusCodeAttribute"/> when set.
    /// <c>null</c> means the handler falls back to the attribute value or 500.
    /// </summary>
    public int? ExplicitStatusCode { get; }

    /// <summary>
    /// Simple class name of the concrete exception type, used as a machine-readable error discriminator.
    /// </summary>
    public string ErrorType { get; }

    /// <summary>
    /// Optional structured payload attached to the error response for additional context.
    /// </summary>
    public object? AdditionalData { get; }

    /// <summary>
    /// Initializes a new instance with the specified message.
    /// </summary>
    /// <param name="message">Human-readable description of the error.</param>
    protected CustomException(string message)
        : this(message, null, null, null) { }

    /// <summary>
    /// Initializes a new instance with the specified message and inner exception.
    /// </summary>
    /// <param name="message">Human-readable description of the error.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    protected CustomException(string message, Exception innerException)
        : this(message, null, null, innerException) { }

    /// <summary>
    /// Initializes a new instance with full control over all exception properties.
    /// </summary>
    /// <param name="message">Human-readable description of the error.</param>
    /// <param name="explicitStatusCode">HTTP status code override; <c>null</c> defers to the attribute.</param>
    /// <param name="additionalData">Optional structured payload included in the error response.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    protected CustomException(string message, int? explicitStatusCode, object? additionalData, Exception? innerException)
        : base(message, innerException)
    {
        ExplicitStatusCode = explicitStatusCode;
        ErrorType = GetType().Name;
        AdditionalData = additionalData;
    }
}
