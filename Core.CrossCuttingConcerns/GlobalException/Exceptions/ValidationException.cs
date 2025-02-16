namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class ValidationException : CustomException
{
    public Dictionary<string, string[]> Errors { get; init; }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    // Eğer string alan constructor varsa kaldırın veya farklı isimde kullanın
    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }
}