namespace Core.CrossCuttingConcerns.GlobalException.Models;

public class ValidationExceptionModel
{
    public string? Property { get; set; }
    public IEnumerable<string>? Errors { get; set; }
    public string? ErrorCode { get; set; }
}