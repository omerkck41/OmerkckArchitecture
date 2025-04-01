﻿using Core.CrossCuttingConcerns.GlobalException.Exceptions;
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

    public static void RegisterHandlersFromAssembly(Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
        .Where(t => t.GetInterfaces()
            .Any(i => i.IsGenericType &&
                     i.GetGenericTypeDefinition() == typeof(IExceptionHandler<>)));

        foreach (var handlerType in handlerTypes)
        {
            var exceptionType = handlerType.GetInterfaces()
                .First(i => i.IsGenericType &&
                           i.GetGenericTypeDefinition() == typeof(IExceptionHandler<>))
                .GetGenericArguments()[0];

            _handlerMappings.TryAdd(exceptionType, handlerType);
        }
    }

    public static void RegisterHandler<TException, THandler>() where TException : Exception where THandler : IExceptionHandler
    {
        _handlerMappings[typeof(TException)] = typeof(THandler);
    }

    public IExceptionHandler GetHandler(Exception exception)
    {
        var exceptionType = exception.GetType();
        var handlerType = FindMostSpecificHandlerType(exceptionType) ?? _defaultHandlerType;

        var handler = _serviceProvider.GetRequiredService(handlerType);

        // Generic handler'lar için özel dönüşüm
        if (handler is IExceptionHandler genericHandler)
        {
            return genericHandler;
        }

        throw new InvalidOperationException($"Handler {handlerType.Name} doesn't implement IExceptionHandler");
    }

    private static Type? FindMostSpecificHandlerType(Type exceptionType)
    {
        while (exceptionType != typeof(Exception))
        {
            if (_handlerMappings.TryGetValue(exceptionType, out var handlerType))
            {
                return handlerType;
            }
            exceptionType = exceptionType.BaseType!;
        }
        return null;
    }
}