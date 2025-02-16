namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class CustomException : Exception
{
    public object? AdditionalData { get; }

    public CustomException(string message) : base(message) { }

    public CustomException(string message, object additionalData) : base(message)
    {
        AdditionalData = additionalData;
    }

    public CustomException(string message, Exception innerException) : base(message, innerException) { }
}