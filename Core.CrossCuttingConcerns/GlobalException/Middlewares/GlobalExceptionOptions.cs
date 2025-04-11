namespace Core.CrossCuttingConcerns.GlobalException.Middlewares;

public class GlobalExceptionOptions
{
    /// <summary>
    /// Hata UI’da oluştuğunda, framework'e göre özelleştirme yapılabilecek merkez davranış.
    /// </summary>
    public Func<HttpContext, Exception, bool, Task>? OnExceptionCompletedAsync { get; set; }
}