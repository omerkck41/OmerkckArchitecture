namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class UnauthorizedException : CustomException
{
    public UnauthorizedException(string message) : base(message) { }
    public UnauthorizedException(string message, Exception innerException) : base(message, innerException) { }
}