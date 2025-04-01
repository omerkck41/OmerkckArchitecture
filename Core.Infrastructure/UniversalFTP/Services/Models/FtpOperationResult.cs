namespace Core.Infrastructure.UniversalFTP.Services.Models;

public class FtpOperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Exception? Exception { get; set; }

    public static FtpOperationResult SuccessResult(string message) => new() { Success = true, Message = message };
    public static FtpOperationResult FailureResult(string message, Exception? exception = null) => new() { Success = false, Message = message, Exception = exception };

    // Global exception standardınıza uyumlu bir hata sonucu oluşturmak için yardımcı metot:
    public static FtpOperationResult FromGlobalException(Exception ex)
    {
        // Global exception yapınız CustomException veya türevlerini kullanıyorsa,
        // buradan ek bilgi (örneğin status code, error type) de alınabilir.
        // Örneğin, global exception handler tarafından üretilen hata mesajı ile FailureResult çağırılır.
        return FailureResult(ex.Message, ex);
    }
}