using System.Diagnostics;

namespace Kck.Core.Abstractions.Results;

/// <summary>Immutable error descriptor that carries a machine-readable code, human-readable message, and a category type.</summary>
/// <param name="Code">Machine-readable error code (e.g. <c>"User.NotFound"</c>).</param>
/// <param name="Message">Human-readable description of the error.</param>
/// <param name="Type">Error category; defaults to <see cref="ErrorType.Failure"/>.</param>
[DebuggerDisplay("[{Type}] {Code}: {Message,nq}")]
public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Failure);

/// <summary>Categorises an <see cref="Error"/> so callers can map it to the appropriate HTTP status or UI message.</summary>
public enum ErrorType
{
    /// <summary>Generic, unclassified failure.</summary>
    Failure,

    /// <summary>Input validation failed (e.g. missing required field, out-of-range value).</summary>
    Validation,

    /// <summary>The requested resource could not be found.</summary>
    NotFound,

    /// <summary>The operation conflicts with the current state of the resource.</summary>
    Conflict,

    /// <summary>The caller is not authenticated.</summary>
    Unauthorized,

    /// <summary>The caller is authenticated but lacks the required permission.</summary>
    Forbidden
}
