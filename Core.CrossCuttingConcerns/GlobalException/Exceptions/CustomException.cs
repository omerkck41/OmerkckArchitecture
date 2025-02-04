namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class CustomException : System.Exception
{
    public CustomException(string message) : base(message) { }
    public CustomException(string message, System.Exception innerException) : base(message, innerException) { }
}