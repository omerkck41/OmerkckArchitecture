namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class BadRequestException : CustomException
{
    public BadRequestException(string message) : base(message) { }
    public BadRequestException(string message, Exception innerException) : base(message, innerException) { }
}