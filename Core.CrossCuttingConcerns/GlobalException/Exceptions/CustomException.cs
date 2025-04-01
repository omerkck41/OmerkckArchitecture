using Microsoft.AspNetCore.Http;

namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class CustomException : Exception
{
    /// <summary>
    /// Eğer oluşturulurken explicit olarak bir durum kodu verildiyse bu property set edilir.
    /// Aksi halde GlobalExceptionHandler, exception türündeki attribute’a bakacaktır.
    /// </summary>
    public int? ExplicitStatusCode { get; }
    public string ErrorType { get; }
    public object? AdditionalData { get; }

    // Sadece mesaj alan temel constructor
    public CustomException(string message)
        : this(message, null, null, null)
    {
    }

    // Mesaj ve innerException alan constructor
    public CustomException(string message, Exception innerException)
        : this(message, null, null, innerException)
    {
    }

    // Parametre adı ve mesaj alan yeni overload
    public CustomException(string paramName, string message)
        : this(message, StatusCodes.Status400BadRequest, new { ParamName = paramName }, null)
    {
    }

    // Protected constructor; explicit status code isteğe bağlı
    protected CustomException(string message, int? explicitStatusCode, object? additionalData, Exception? innerException)
        : base(message, innerException)
    {
        ExplicitStatusCode = explicitStatusCode;
        ErrorType = GetType().Name;
        AdditionalData = additionalData;
    }
}