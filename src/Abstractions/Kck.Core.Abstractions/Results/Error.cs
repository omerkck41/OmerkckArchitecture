namespace Kck.Core.Abstractions.Results;

public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Failure);

public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden
}
