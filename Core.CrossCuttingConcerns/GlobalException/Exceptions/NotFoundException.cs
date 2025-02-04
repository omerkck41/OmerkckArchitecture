namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class NotFoundException : CustomException
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string message, System.Exception innerException) : base(message, innerException) { }
}