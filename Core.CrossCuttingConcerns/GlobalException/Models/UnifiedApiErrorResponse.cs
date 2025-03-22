namespace Core.CrossCuttingConcerns.GlobalException.Models;

public class UnifiedApiErrorResponse
{
    public bool Success { get; set; } = false; // Hata durumunda her zaman false
    public string Message { get; set; } = string.Empty; // Kısa hata mesajı
    public int StatusCode { get; set; } // HTTP status code
    public string? ErrorType { get; set; } // Exception tipi
    public string? Detail { get; set; } // Detaylı hata mesajı (örneğin validation hataları veya ek veri)
    public object? AdditionalData { get; set; } // İsteğe bağlı ek veri (örneğin, CustomException içindeki ek bilgiler)
}
