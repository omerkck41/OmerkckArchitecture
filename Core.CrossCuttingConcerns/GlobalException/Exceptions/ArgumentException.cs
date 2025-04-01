namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class ArgumentException : CustomException
{
    public ArgumentException(string message) : base(message) { }
    public ArgumentException(string message, Exception innerException) : base(message, innerException) { }
}