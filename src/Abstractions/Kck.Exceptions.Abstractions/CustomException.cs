namespace Kck.Exceptions;

public abstract class CustomException : Exception
{
    public int? ExplicitStatusCode { get; }
    public string ErrorType { get; }
    public object? AdditionalData { get; }

    protected CustomException(string message)
        : this(message, null, null, null) { }

    protected CustomException(string message, Exception innerException)
        : this(message, null, null, innerException) { }

    protected CustomException(string message, int? explicitStatusCode, object? additionalData, Exception? innerException)
        : base(message, innerException)
    {
        ExplicitStatusCode = explicitStatusCode;
        ErrorType = GetType().Name;
        AdditionalData = additionalData;
    }
}
