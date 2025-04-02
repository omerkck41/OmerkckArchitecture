using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;
using InvalidOperationException = Core.CrossCuttingConcerns.GlobalException.Exceptions.CustomInvalidOperationException;

namespace Core.CrossCuttingConcerns.GlobalException.Handlers;

public class ExceptionHandlerFactory : IExceptionHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentDictionary<Type, Type> _handlerMappings = new();
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
                HandlerType = i.GetGenericArguments()[0], // Exception type
                ImplementationType = i.DeclaringType     // Handler type
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
                $"Handler bulunamadı: {handlerType.Name}. DI container'a kayıtlı olduğundan emin olun. " +
                $"Exception: {ex.Message}");
        }
    }

    private static Type? FindMostSpecificHandlerType(Type exceptionType)
    {
        while (exceptionType != null && exceptionType != typeof(Exception))
        {
            if (_handlerMappings.TryGetValue(exceptionType, out var handlerType))
            {
                return handlerType;
            }
            exceptionType = exceptionType.BaseType;
        }
        return null;
    }
}