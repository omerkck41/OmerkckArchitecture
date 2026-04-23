using Kck.Exceptions.Attributes;

namespace Kck.Exceptions;

[HttpStatusCode(400)]
public class CustomArgumentException : CustomException
{
    public CustomArgumentException(string message, string? paramName = null)
        : base(message, null, paramName is not null ? new { ParamName = paramName } : null, null) { }

    public CustomArgumentException(string message, Exception innerException)
        : base(message, innerException) { }
}
