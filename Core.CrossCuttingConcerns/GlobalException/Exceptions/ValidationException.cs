namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class ValidationException : CustomException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    // Hata mesajını otomatik olarak Errors ile birleştir
    public override string ToString()
    {
        var errorMessages = string.Join("; ",
            Errors.Select(e => $"{e.Key}: {string.Join(", ", e.Value)}")
        );
        return $"{base.Message} - Details: {errorMessages}";
    }
}