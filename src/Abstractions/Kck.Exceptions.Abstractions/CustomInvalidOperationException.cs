using Kck.Exceptions.Attributes;

namespace Kck.Exceptions;

[HttpStatusCode(400)]
public class CustomInvalidOperationException : CustomException
{
    public CustomInvalidOperationException(string message)
        : base(message) { }

    public CustomInvalidOperationException(string message, Exception innerException)
        : base(message, innerException) { }
}
