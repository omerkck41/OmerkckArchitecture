namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class TimeoutException : CustomException
{
    public TimeoutException(string message) : base(message) { }
    public TimeoutException(string message, Exception innerException) : base(message, innerException) { }
}