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
        // İleride başka özel hata işleyiciler eklemek isterseniz, switch-case içerisine ekleyebilirsiniz.
        return exception switch
        {
            ValidationException => _serviceProvider.GetRequiredService<ValidationExceptionHandler>(),
            _ => _serviceProvider.GetRequiredService<GlobalExceptionHandler>()
        };
    }
}