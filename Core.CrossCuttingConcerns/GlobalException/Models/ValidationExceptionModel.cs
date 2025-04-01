namespace Core.CrossCuttingConcerns.GlobalException.Models;

public class ValidationExceptionModel
{
    public string Property { get; set; } = string.Empty;
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();
}