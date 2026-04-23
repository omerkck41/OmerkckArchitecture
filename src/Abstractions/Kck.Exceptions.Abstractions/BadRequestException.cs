using Kck.Exceptions.Attributes;

namespace Kck.Exceptions;

[HttpStatusCode(400)]
public class BadRequestException : CustomException
{
    public BadRequestException(string message)
        : base(message) { }

    public BadRequestException(string message, object additionalData)
        : base(message, null, additionalData, null) { }

    public BadRequestException(string message, Exception innerException)
        : base(message, innerException) { }
}
