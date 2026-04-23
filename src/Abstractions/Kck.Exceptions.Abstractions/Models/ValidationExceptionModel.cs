namespace Kck.Exceptions.Models;

public sealed class ValidationExceptionModel
{
    public string Property { get; init; } = string.Empty;
    public IEnumerable<string> Errors { get; init; } = [];
}
