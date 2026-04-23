using Kck.Exceptions.Attributes;

namespace Kck.Exceptions;

[HttpStatusCode(401)]
public class UnauthorizedException : CustomException
{
    public UnauthorizedException(string message = "Authentication required")
        : base(message) { }

    public UnauthorizedException(string message, Exception innerException)
        : base(message, innerException) { }
}
