namespace Kck.Exceptions.AspNetCore.Handlers;

/// <summary>
/// Verilen exception tipine uygun <see cref="IKckExceptionHandler"/>'ı çözer.
/// ValidationExceptionHandler (en spesifik) ilk kontrol edilir, ardından GlobalExceptionHandler.
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
    /// Exception tipine göre en uygun handler'ı döner.
    /// </summary>
    public IKckExceptionHandler GetHandler(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        // En spesifik handler ilk kontrol edilir
        if (_validationHandler.CanHandle(exception))
        {
            return _validationHandler;
        }

        if (_globalHandler.CanHandle(exception))
        {
            return _globalHandler;
        }

        // CustomException olmayan hatalar da GlobalExceptionHandler tarafından işlenir (fallback)
        return _globalHandler;
    }
}
