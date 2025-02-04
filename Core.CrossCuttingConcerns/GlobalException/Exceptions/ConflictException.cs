namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class ConflictException : CustomException
{
    public ConflictException(string message) : base(message) { }
    public ConflictException(string message, System.Exception innerException) : base(message, innerException) { }
}