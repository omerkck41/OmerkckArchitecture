using Kck.Exceptions.Attributes;

namespace Kck.Exceptions;

[HttpStatusCode(408)]
public class RequestTimeoutException : CustomException
{
    public RequestTimeoutException(string message = "Request timed out")
        : base(message) { }

    public RequestTimeoutException(string message, Exception innerException)
        : base(message, innerException) { }
}
