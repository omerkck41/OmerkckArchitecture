using Microsoft.AspNetCore.Http;

namespace Core.CrossCuttingConcerns.GlobalException.Handlers;

public interface IExceptionHandler
{
    Task HandleExceptionAsync(HttpContext context, System.Exception exception);
}