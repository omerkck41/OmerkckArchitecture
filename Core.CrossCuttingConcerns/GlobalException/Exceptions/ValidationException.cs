using System.Text.Json;

namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class ValidationException : CustomException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(Dictionary<string, string[]> errors) : base(CreateErrorMessage(errors))
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    private static string CreateErrorMessage(Dictionary<string, string[]> errors)
    {
        return JsonSerializer.Serialize(errors, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}