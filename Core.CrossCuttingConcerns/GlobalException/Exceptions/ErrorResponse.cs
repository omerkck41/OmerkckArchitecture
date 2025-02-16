namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class ErrorResponse
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public object? Errors { get; set; }
    public string? ErrorCode { get; set; }
    public string? CorrelationId { get; set; }
}