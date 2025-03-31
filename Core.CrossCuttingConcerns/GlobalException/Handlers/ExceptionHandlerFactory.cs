using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Core.CrossCuttingConcerns.GlobalException.Handlers;

public class ExceptionHandlerFactory : IExceptionHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly Dictionary<Type, Type> ExceptionHandlerMapping = new()
    {
        // Özel handler eşlemeleri: eğer ValidationException fırlatılırsa, ValidationExceptionHandler kullanılacak.
        { typeof(ValidationException), typeof(ValidationExceptionHandler) }
        // İleride diğer exception tiplerini de ekleyebilirsiniz.
    };

    public ExceptionHandlerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IExceptionHandler GetHandler(Exception exception)
    {
        var exceptionType = exception.GetType();
        var handlerType = ExceptionHandlerMapping
            .FirstOrDefault(mapping => mapping.Key.IsAssignableFrom(exceptionType)).Value;

        if (handlerType != null)
        {
            var handler = _serviceProvider.GetServices<IExceptionHandler>()
                .FirstOrDefault(h => h.GetType() == handlerType);

            if (handler != null)
                return handler;
        }

        // Eğer eşleşen özel handler bulunamazsa, GlobalExceptionHandler'ı döndür.
        var globalHandler = _serviceProvider.GetServices<IExceptionHandler>()
            .OfType<GlobalExceptionHandler>().FirstOrDefault();

        if (globalHandler != null)
            return globalHandler;

        throw new Exception("GlobalExceptionHandler is not registered.");
    }
}