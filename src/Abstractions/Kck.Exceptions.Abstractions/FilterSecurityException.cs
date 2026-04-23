using Kck.Exceptions.Attributes;

namespace Kck.Exceptions;

[HttpStatusCode(403)]
public class FilterSecurityException : CustomException
{
    public FilterSecurityException(string propertyName)
        : base($"Filtering on property '{propertyName}' is not allowed.") { }

    public FilterSecurityException(string propertyName, string operatorName)
        : base($"Operator '{operatorName}' is not allowed on property '{propertyName}'.") { }
}
