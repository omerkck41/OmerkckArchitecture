namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class ValidationException : CustomException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    // Hata detaylarını ToString üzerinden dön
    public override string ToString()
    {
        return System.Text.Json.JsonSerializer.Serialize(Errors, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}