namespace Kck.Exceptions.AspNetCore.Handlers;

/// <summary>
/// Resolves the <see cref="IKckExceptionHandler"/> that matches the given exception type.
/// ValidationExceptionHandler (the most specific) is checked first, then GlobalExceptionHandler.
/// </summary>
public sealed class ExceptionHandlerFactory
{
    private readonly ValidationExceptionHandler _validationHandler;
    private readonly GlobalExceptionHandler _globalHandler;

    public ExceptionHandlerFactory(
        ValidationExceptionHandler validationHandler,
        GlobalExceptionHandler globalHandler)
    {
        _validationHandler = validationHandler;
        _globalHandler = globalHandler;
    }

    /// <summary>
    /// Returns the most suitable handler for the exception type.
    /// </summary>
    public IKckExceptionHandler GetHandler(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        // The most specific handler is checked first
        if (_validationHandler.CanHandle(exception))
        {
            return _validationHandler;
        }

        if (_globalHandler.CanHandle(exception))
        {
            return _globalHandler;
        }

        // Errors that are not a CustomException are also handled by GlobalExceptionHandler (fallback)
        return _globalHandler;
    }
}
