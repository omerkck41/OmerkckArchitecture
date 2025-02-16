namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class ConflictException : CustomException
{
    public ConflictException(string message) : base(message) { }
    public ConflictException(string message, Exception innerException) : base(message, innerException) { }
}