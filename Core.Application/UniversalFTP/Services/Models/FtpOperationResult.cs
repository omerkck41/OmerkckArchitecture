namespace Core.Application.UniversalFTP.Services.Models;

public class FtpOperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Exception? Exception { get; set; }

    public static FtpOperationResult SuccessResult(string message) => new() { Success = true, Message = message };
    public static FtpOperationResult FailureResult(string message, Exception? exception = null) => new() { Success = false, Message = message, Exception = exception };
}