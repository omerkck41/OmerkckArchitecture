using Kck.Exceptions.Attributes;

namespace Kck.Exceptions;

[HttpStatusCode(403)]
public class ForbiddenException : CustomException
{
    public ForbiddenException(string message = "Insufficient permissions")
        : base(message) { }

    public ForbiddenException(string message, Exception innerException)
        : base(message, innerException) { }
}
