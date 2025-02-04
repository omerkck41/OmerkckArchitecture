namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class ErrorResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string Detail { get; set; }
    public string Type { get; set; }
    public Dictionary<string, string[]> Errors { get; set; }
    public string StackTrace { get; set; }
    public int? ErrorCode { get; set; } // Özel hata kodu
    public string CorrelationId { get; set; } // İstek kimliği
    public DateTime Timestamp { get; set; } // Zaman damgası
}