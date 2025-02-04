namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class ValidationException : CustomException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException() : base("One or more validation failures have occurred.")
    {
        Errors = [];
    }

    public ValidationException(Dictionary<string, string[]> errors) : this()
    {
        Errors = errors;
    }
}