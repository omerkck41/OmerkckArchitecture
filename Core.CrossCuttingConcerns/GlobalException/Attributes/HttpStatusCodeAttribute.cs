namespace Core.CrossCuttingConcerns.GlobalException.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class HttpStatusCodeAttribute(int statusCode) : Attribute
{
    public int StatusCode { get; } = statusCode;
}