using Microsoft.AspNetCore.Http;

namespace Core.CrossCuttingConcerns.GlobalException.Handlers;

public interface IExceptionHandler
{
    Task HandleExceptionAsync(HttpContext context, Exception exception);
}

public interface IExceptionHandler<TException> : IExceptionHandler where TException : Exception
{
    Task HandleExceptionAsync(HttpContext context, TException exception);
}