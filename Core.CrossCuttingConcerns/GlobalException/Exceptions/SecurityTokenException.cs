namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class SecurityTokenException : CustomException
{
    public SecurityTokenException(string message) : base(message) { }
    public SecurityTokenException(string message, Exception innerException) : base(message, innerException) { }
}
