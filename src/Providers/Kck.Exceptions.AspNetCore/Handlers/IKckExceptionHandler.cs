using Kck.Exceptions.AspNetCore.Models;
using Microsoft.AspNetCore.Http;

namespace Kck.Exceptions.AspNetCore.Handlers;

/// <summary>
/// KCK exception handler interface.
/// Each handler can process specific exception types.
/// </summary>
public interface IKckExceptionHandler
{
    /// <summary>
    /// Determines whether this handler can process the given exception.
    /// </summary>
    bool CanHandle(Exception exception);

    /// <summary>
    /// Processes the exception and builds an RFC 7807 compliant error response.
    /// </summary>
    UnifiedApiErrorResponse Handle(Exception exception, HttpContext context);
}
