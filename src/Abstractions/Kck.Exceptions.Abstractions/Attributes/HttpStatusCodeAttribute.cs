namespace Kck.Exceptions.Attributes;

/// <summary>
/// Maps an exception type to an HTTP status code for GlobalExceptionHandler routing.
/// Apply to a <see cref="System.Exception"/> subclass to declare the default response status.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class HttpStatusCodeAttribute(int statusCode) : Attribute
{
    /// <summary>
    /// The HTTP status code to return when this exception type is caught.
    /// </summary>
    public int StatusCode { get; } = statusCode;
}
