namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class ValidationException : CustomException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation failures have occurred.", errors)
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }
}