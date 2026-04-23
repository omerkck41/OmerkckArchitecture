using Kck.Exceptions.Attributes;

namespace Kck.Exceptions;

[HttpStatusCode(409)]
public class ConflictException : CustomException
{
    public ConflictException(string message = "Conflict occurred")
        : base(message) { }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException) { }
}
