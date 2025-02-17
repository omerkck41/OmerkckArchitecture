using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Core.CrossCuttingConcerns.GlobalException.Handlers;

public class ExceptionHandlerFactory : IExceptionHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ExceptionHandlerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IExceptionHandler GetHandler(Exception exception)
    {
        var handlers = _serviceProvider.GetServices<IExceptionHandler>(); // çoklu implementasyon döner

        return exception switch
        {
            ValidationException => handlers.OfType<ValidationExceptionHandler>().First(),
            _ => handlers.OfType<GlobalExceptionHandler>().First()
        };
    }
}