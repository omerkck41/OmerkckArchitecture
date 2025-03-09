# OmerkckArchitecture Api ControllerBase Kütüphanesi

Bu kütüphane, ASP.NET Core API geliştirme sürecini kolaylaştırmak için tasarlanmıştır. **ApiResponse**, **Controller Base**, **Service Extensions** ve diğer yardımcı sınıfları içerir.

## İçindekiler
- [Kurulum](#kurulum)
- [Kullanım](#kullanım)
  - [Service Bağımlılıklarını Eklemek](#service-bağımlılıklarını-eklemek)
  - [API Controller Kullanımı](#api-controller-kullanımı)
  - [ApiResponse Kullanımı](#apiresponse-kullanımı)
  - [Helper Metodlar](#helper-metodlar)

---

## Kurulum

Bu kütüphaneyi projenize eklemek için **NuGet** veya manuel referans ekleme yöntemini kullanabilirsiniz.

### 1. Manuel Yükleme

Projenize **Core.Api.ApiControllerBase** isimli klasör oluşturun ve aşağıdaki dosyaları ekleyin:
- `ApiControllerBase.cs`
- `ServiceCollectionExtensions.cs`
- `ApiResponseHelper.cs`
- `ApiResponse.cs`
- `ApiResponseExtensions.cs`
- `HttpContextExtensions.cs`
- `LocationHelper.cs`

Ardından proje dosyanızı tekrar derleyin.

---

## Kullanım

### Service Bağımlılıklarını Eklemek

`ServiceCollectionExtensions.cs` dosyasında yer alan `AddApiHelperLibrary` metodu, servislere ihtiyaç duyulan bağımlılıkları eklemek için kullanılır:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddApiHelperLibrary();
}
```

---

### API Controller Kullanımı

**ApiControllerBase**, API controller'ların ortak davranışlarını içerir. API controller'larınızı bu sınıftan türeterek **ApiResponse** dönüşen metodlar oluşturabilirsiniz.

#### Kullanım
```csharp
[Route("api/[controller]")]
[ApiController]
public class ProductsController : ApiControllerBase
{
    [HttpGet]
    public IActionResult GetProducts()
    {
        var products = new List<string> { "Laptop", "Telefon", "Tablet" };
        var response = ApiResponseHelper.CreateSuccessResponse(products, "Ürünler getirildi", HttpContext);
        return HandleResult(response);
    }
}
```

---

### ApiResponse Kullanımı

ApiResponse sınıfı, API dönüslerini standart bir yapıya oturtmak için tasarlanmıştır. **Başarılı ve başarısız yanıtlar için kullanılabilir.**

#### ApiResponse Kullanımı

**Başarılı yanıt:**
```csharp
var successResponse = ApiResponseHelper.CreateSuccessResponse(data, "Başarılı", HttpContext);
return successResponse.ToActionResult(HttpContext);
```

**Başarısız yanıt:**
```csharp
var errorResponse = ApiResponseHelper.CreateFailResponse<object>("Bir hata oluştu", HttpContext);
return errorResponse.ToActionResult(HttpContext);
```

---

### Helper Metodlar

Kütüphanede yer alan yardımcı metodlar, API iş akışlarınızı düzenlemek için tasarlanmıştır.

#### LocationHelper
Bu sınıf, yeni oluşturulan kaynaklar için `Location` header'ları oluşturur.

**Kullanım:**
```csharp
string location = LocationHelper.CreateLocationHeader(HttpContext.Request, resourceId);
```

#### HttpContextExtensions
Bu sınıf, `HttpContext` üzerinden ekstra işlemler yapmak için oluşturulmuştur. (Geliştirilmeye devam ediyor.)

---