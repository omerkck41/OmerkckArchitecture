namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class InvalidOperationException : CustomException
{
    public InvalidOperationException(string message) : base(message) { }
    public InvalidOperationException(string message, Exception innerException) : base(message, innerException) { }
}
