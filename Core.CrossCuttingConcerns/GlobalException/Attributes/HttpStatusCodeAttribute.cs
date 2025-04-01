namespace Core.CrossCuttingConcerns.GlobalException.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class HttpStatusCodeAttribute : Attribute
{
    public int StatusCode { get; }
    public HttpStatusCodeAttribute(int statusCode)
    {
        StatusCode = statusCode;
    }
}