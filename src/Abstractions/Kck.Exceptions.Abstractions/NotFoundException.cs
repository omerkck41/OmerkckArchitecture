using Kck.Exceptions.Attributes;

namespace Kck.Exceptions;

[HttpStatusCode(404)]
public class NotFoundException : CustomException
{
    public NotFoundException(string message)
        : base(message) { }

    public NotFoundException(string message, object additionalData)
        : base(message, null, additionalData, null) { }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
}
