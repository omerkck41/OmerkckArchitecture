using Kck.Exceptions.Attributes;

namespace Kck.Exceptions;

[HttpStatusCode(429)]
public class TooManyRequestsException : CustomException
{
    public TooManyRequestsException(string message = "Too many requests")
        : base(message) { }

    public TooManyRequestsException(string message, Exception innerException)
        : base(message, innerException) { }
}
