using Kck.Exceptions.Attributes;

namespace Kck.Exceptions;

[HttpStatusCode(401)]
public class SecurityTokenException : CustomException
{
    public SecurityTokenException(string message)
        : base(message) { }

    public SecurityTokenException(string message, Exception innerException)
        : base(message, innerException) { }
}
