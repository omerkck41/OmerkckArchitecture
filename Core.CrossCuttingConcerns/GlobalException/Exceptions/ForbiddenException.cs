namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class ForbiddenException : CustomException
{
    public ForbiddenException(string message) : base(message) { }
    public ForbiddenException(string message, Exception innerException) : base(message, innerException) { }
}