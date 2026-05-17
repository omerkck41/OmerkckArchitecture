using Kck.Exceptions.Attributes;

namespace Kck.Exceptions;

/// <summary>
/// Thrown when a dynamic filter request references a property or operator that is not whitelisted.
/// Results in an HTTP 403 response via GlobalExceptionHandler to prevent unauthorized data probing.
/// </summary>
[HttpStatusCode(403)]
public class FilterSecurityException : CustomException
{
    /// <summary>
    /// Initializes a new instance indicating that filtering on the given property is not allowed.
    /// </summary>
    /// <param name="propertyName">The name of the property that was rejected.</param>
    public FilterSecurityException(string propertyName)
        : base($"Filtering on property '{propertyName}' is not allowed.") { }

    /// <summary>
    /// Initializes a new instance indicating that a specific operator is not allowed on the given property.
    /// </summary>
    /// <param name="propertyName">The name of the property that was targeted.</param>
    /// <param name="operatorName">The name of the operator that was rejected.</param>
    public FilterSecurityException(string propertyName, string operatorName)
        : base($"Operator '{operatorName}' is not allowed on property '{propertyName}'.") { }
}
