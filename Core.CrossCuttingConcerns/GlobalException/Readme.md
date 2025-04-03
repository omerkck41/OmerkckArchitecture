# GlobalExceptionHandler Kütüphanesi

.NET 9.0 ve üzeri için geliştirilmiş bu kütüphane, ASP.NET Core API uygulamalarında standart, genellenebilir ve özelleştirilebilir bir hata yönetimi sunar. Exception handling yapısı RFC 7807 (ProblemDetails) standardına uygundur ve özellikle geliştiricilere temiz, loglanabilir ve ölçeklenebilir bir hata yapısı sağlar.

## Özellikler

- ✨ RFC 7807 uyumlu `UnifiedApiErrorResponse` yapısı
- ✨ Tüm hata mesajları tek formatta: statusCode, message, detail, additionalData
- ✨ Validation hataları için `ValidationException` desteği
- ✨ Custom Exception tanımlama desteği (NotFoundException, TimeoutException, vb.)
- ✨ Attribute tabanlı HTTP Status Code desteği
- ✨ Global Exception Middleware ile uygulama seviyesinde hata yakalama
- ✨ HandlerFactory ile Exception bazlı handler yönlendirme
- ✨ ProblemDetails + Swagger/OpenAPI uyumlu hata dönüşleri
- ✨ Localization desteği için hazır altyapı (isteğe bağlı)

---

## Kurulum

### 1. Projeye Submodule Ekleme

```bash
git submodule add https://github.com/omerkck41/OmerkckArchitecture/tree/master/Core.CrossCuttingConcerns/GlobalException src/Core/CrossCuttingConcerns/GlobalException
```

### 2. Startup / Program.cs İçerisinde Servisleri Kaydetme

```csharp
builder.Services.AddAdvancedExceptionHandling();
```

> `Resources` klasörü kütüphane içindeyse, localization desteği de otomatik olarak eklenir.

### 3. Middleware Ekleme

```csharp
app.UseAdvancedExceptionHandling();
```

---

## Kullanım

### 1. Controller içinde Exception fırlatma

```csharp
[HttpGet("{id}")]
public IActionResult Get(int id)
{
    if (id <= 0)
        throw new BadRequestException("ID must be greater than zero.");

    throw new NotFoundException("User not found.");
}
```

### 2. ValidationException Otomatik Format

```json
{
  "errorId": "...",
  "success": false,
  "statusCode": 400,
  "title": "Validation Error",
  "message": "Validation error",
  "errorType": "ValidationException",
  "detail": "Validation failed for one or more fields.",
  "additionalData": [
    {
      "property": "Email",
      "errors": [
        "Email is required.",
        "Email must be valid."
      ]
    }
  ]
}
```

---

## Custom Exception Tanımlama

```csharp
[HttpStatusCode(StatusCodes.Status429TooManyRequests)]
public class TooManyRequestsException : CustomException
{
    public TooManyRequestsException(string message)
        : base(message) { }
}
```

---

## Custom Handler Yazmak

```csharp
public class MyCustomExceptionHandler : IExceptionHandler<MyCustomException>
{
    public async Task HandleExceptionAsync(HttpContext context, MyCustomException exception)
    {
        var response = UnifiedApiErrorResponse.FromException(exception) with
        {
            StatusCode = StatusCodes.Status400BadRequest,
            Message = "Special error: " + exception.Message
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = response.StatusCode;
        await context.Response.WriteAsJsonAsync(response);
    }
}
```

---

## Swagger/OpenAPI Uyumu

ProblemDetails middleware ile uyumludur. Aşağıdaki gibi controller'a belirtilebilir:

```csharp
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(UnifiedApiErrorResponse), StatusCodes.Status500InternalServerError)]
```

---

## Geliştirici Notları

- Hatalar tek tipte (record-based response) döner.
- Custom exception veya handler eklemek kolaydır.
- Çoklu handler desteği (Validation, Auth, Token vb.) mevcut.
- Proje için sorumluluklar ayrı, modüler net.

---

## Önerilen Klasör Yapısı

```
Core/
  CrossCuttingConcerns/
    GlobalException/
      Attributes/
      Exceptions/
      Extensions/
      Handlers/
      Middlewares/
      Models/
      Resources/
        ErrorMessages.resx
        ErrorMessages.tr.resx
        ErrorMessages.en.resx
```

---

