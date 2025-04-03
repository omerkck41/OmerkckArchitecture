using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using InvalidOperationException = Core.CrossCuttingConcerns.GlobalException.Exceptions.CustomInvalidOperationException;

namespace Core.CrossCuttingConcerns.GlobalException.Handlers;

public class ExceptionHandlerFactory : IExceptionHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentDictionary<Type, Type> _handlerMappings = new();
    // Yeni cache: Exception tipi -> Handler tipi (null ise _defaultHandlerType kullanılacak)
    private static readonly ConcurrentDictionary<Type, Type?> _handlerCache = new();
    private static readonly Type _defaultHandlerType = typeof(GlobalExceptionHandler);

    static ExceptionHandlerFactory()
    {
        // Ön tanımlı eşlemeler
        RegisterHandler<ValidationException, ValidationExceptionHandler>();
    }

    public ExceptionHandlerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Belirtilen assembly'deki tüm IExceptionHandler<T> implementasyonlarını otomatik olarak kaydeder
    /// </summary>
    /// <param name="assembly">Taranacak assembly</param>
    /// <param name="overwriteExisting">Varolan kayıtların üzerine yazılsın mı?</param>
    public static void RegisterHandlersFromAssembly(Assembly assembly, bool overwriteExisting = false)
    {
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly));

        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                           i.GetGenericTypeDefinition() == typeof(IExceptionHandler<>))
            .Select(i => new
            {
                HandlerType = i.GetGenericArguments()[0], // Exception tipi
                ImplementationType = t     // Handler tipi (DeclaringType yerine t kullanılabilir)
            }));

        foreach (var handler in handlerTypes)
        {
            if (overwriteExisting)
            {
                _handlerMappings.AddOrUpdate(
                    handler.HandlerType,
                    handler.ImplementationType,
                    (_, __) => handler.ImplementationType);
            }
            else
            {
                _handlerMappings.TryAdd(handler.HandlerType, handler.ImplementationType);
            }
        }
    }

    public static void RegisterHandler<TException, THandler>()
        where TException : Exception
        where THandler : IExceptionHandler
    {
        _handlerMappings[typeof(TException)] = typeof(THandler);
    }

    public IExceptionHandler GetHandler(Exception exception)
    {
        if (exception == null)
            throw new ArgumentNullException(nameof(exception));

        var exceptionType = exception.GetType();
        var handlerType = FindMostSpecificHandlerType(exceptionType) ?? _defaultHandlerType;

        try
        {
            return (IExceptionHandler)_serviceProvider.GetRequiredService(handlerType);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Handler bulunamadı: {handlerType.Name}. DI container'a kaydedildiğinden emin olun. " +
                $"Exception: {ex.Message}");
        }
    }

    private static Type? FindMostSpecificHandlerType(Type exceptionType)
    {
        // Cache kontrolü
        if (_handlerCache.TryGetValue(exceptionType, out var cachedHandler))
        {
            return cachedHandler;
        }

        Type? handlerType = null;
        var currentType = exceptionType;
        while (currentType != null && currentType != typeof(Exception))
        {
            if (_handlerMappings.TryGetValue(currentType, out handlerType))
            {
                break;
            }
            currentType = currentType.BaseType;
        }
        _handlerCache.TryAdd(exceptionType, handlerType);
        return handlerType;
    }
}