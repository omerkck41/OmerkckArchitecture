namespace Core.CrossCuttingConcerns.GlobalException.Middlewares;

public class GlobalExceptionOptions
{
    public Func<HttpContext, Exception, Task>? OnHtmlException { get; set; }
}