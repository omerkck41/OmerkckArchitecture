# Core.API - Kullanım ve Entegrasyon Rehberi

## **Nedir?**
Core.API, projelerde standart bir API altyapısı sunmak için tasarlanmış, modüler bir **Class Library**'dir. **GlobalExceptionMiddleware**, **ValidationFilter**, **Swagger Entegrasyonu** gibi özelliklerle hızlı ve tutarlı bir API geliştirme deneyimi sağlar.

---

## **Neden Kullanılır?**
Core.API, büyük projelerde tekrar eden API yapılarını merkezileştirerek:
- **Kod Tekrarını Azaltır**: Tek bir merkezi kütüphaneyle tüm projelerde ortak özellikler kullanılır.
- **Standartlaştırır**: Projeler arasında tutarlı bir API altyapısı sağlar.
- **Zaman Kazandırır**: Hazır yapı taşları ile geliştirme sürecini hızlandırır.

---

## **Avantajları**
1. **Modüler ve Esnek Yapı**:
   - Ortak API bileşenlerini (Middleware, Filter, Extensions) modüler olarak sunar.

2. **Kolay Entegrasyon**:
   - Hızlı bir şekilde projelere entegre edilebilir.

3. **Global Hata Yönetimi**:
   - **GlobalExceptionMiddleware** ile tüm hataları merkezi bir yerden yönetir.

4. **Swagger Desteği**:
   - Swagger ile API'lerinizi kolayca belgeleyebilir ve test edebilirsiniz.

---

## **Projeye Entegrasyon**

### **1. Gerekli NuGet Paketlerinin Kurulumu**
Core.API modülünü kullanabilmek için aşağıdaki NuGet paketlerini ekleyin:

```bash
Install-Package Microsoft.AspNetCore.Mvc
Install-Package Microsoft.Extensions.DependencyInjection
Install-Package Microsoft.Extensions.Logging
Install-Package Swashbuckle.AspNetCore
Install-Package System.Text.Json
```

---

### **2. Program.cs Yapılandırması**
Core.API modülünü projenize eklemek için aşağıdaki kodları kullanın:

#### **Servislerin Eklenmesi**
```csharp
using Core.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Core.API servislerini ekle
builder.Services.AddApiServices();
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

// Middleware'leri ekle
app.UseApiMiddlewares();
app.UseSwaggerDocumentation();

// Endpoint'leri tanımla
app.MapControllers();

app.Run();
```

---

### **3. appsettings.json Yapılandırması**
Eğer API anahtarlarını kontrol etmek veya diğer API yapılandırmalarını kullanmak istiyorsanız `appsettings.json` dosyasına aşağıdaki ayarları ekleyin:

```json
{
  "ApiSettings": {
    "ApiKey": "YourSecureApiKey"
  }
}
```

---

## **Detaylı Kullanım Örnekleri**

### **1. BaseApiController Kullanımı**
Tüm Controller'larınız **BaseApiController**'dan türetilebilir ve standart API cevap formatını kullanabilir:

```csharp
using Core.API.Controllers;

namespace Example.API.Controllers;

public class ProductsController : BaseApiController
{
    [HttpGet]
    public IActionResult GetProducts()
    {
        var products = new[] { "Laptop", "Tablet", "Phone" };
        return ApiResponse(products, "Products retrieved successfully");
    }
}
```

---

### **2. GlobalExceptionMiddleware Kullanımı**
Hataları merkezi bir yerden yönetmek için `GlobalExceptionMiddleware` kullanılır:
```csharp
// Program.cs içinde
app.UseApiMiddlewares();
```
Bu middleware, tüm hataları JSON formatında döndürür:
```json
{
  "success": false,
  "message": "Internal Server Error"
}
```

---

### **3. ValidationFilter Kullanımı**
Model validasyon hatalarını otomatik olarak kontrol etmek için `ValidationFilter` kullanılır:
```csharp
// Program.cs içinde
builder.Services.AddApiServices();

// Model Örneği
public class ProductRequest
{
    [Required]
    public string Name { get; set; }

    [Range(1, 1000)]
    public decimal Price { get; set; }
}

// Controller Örneği
[HttpPost]
public IActionResult CreateProduct([FromBody] ProductRequest request)
{
    return ApiResponse(request, "Product created successfully");
}
```
Eğer bir validasyon hatası oluşursa:
```json
{
  "success": false,
  "errors": [
    { "key": "Name", "errors": ["The Name field is required."] }
  ]
}
```

---

### **4. Swagger Kullanımı**
API'lerinizi belgelemek ve test etmek için Swagger kullanabilirsiniz:
```csharp
// Program.cs içinde
builder.Services.AddSwaggerDocumentation();
app.UseSwaggerDocumentation();
```
Swagger arayüzüne `/swagger/index.html` üzerinden erişebilirsiniz.

---

## **Sonuç**
Core.API, projelerdeki API geliştirme sürecini hızlandırmak ve standartlaştırmak için güçlü bir çözüm sunar.
Modüler yapısı sayesinde kolayca entegre edilebilir ve büyük projelerde API yönetimini basitleştirir.
