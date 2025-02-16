namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class ValidationException : CustomException
{
    public Dictionary<string, string[]> Errors { get; init; } = [];

    public ValidationException() : base("One or more validation failures have occurred.")
    {
    }

    public ValidationException(Dictionary<string, string[]> errors) : this()
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }
}