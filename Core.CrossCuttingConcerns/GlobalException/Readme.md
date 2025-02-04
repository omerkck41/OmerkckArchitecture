---

# Core.CrossCuttingConcerns

## **Nedir?**
**Core.CrossCuttingConcerns**, .NET Core tabanlı uygulamalarda merkezi bir hata yönetimi mekanizması sağlayan bir kütüphanedir.
Bu kütüphane, uygulamanızda oluşabilecek hataları yakalar, loglar ve kullanıcıya anlamlı hata yanıtları döner. Ayrıca, özel hata türleri ve validasyon hataları için destek sunar.

---

## **Neden Kullanılır?**
- **Merkezi Hata Yönetimi:** Tüm hataları tek bir yerden yönetebilirsiniz.
- **Tutarlı Hata Yanıtları:** Tüm hatalar, standart bir formatla kullanıcıya döner.
- **Geliştirici Dostu:** Hata mesajları, stack trace ve diğer detaylar, geliştirme ortamında kolayca görüntülenebilir.
- **Genişletilebilirlik:** Yeni hata türleri ve hata işleyiciler ekleyebilirsiniz.
- **Loglama:** Hataları otomatik olarak loglayarak, sorunları daha kolay tespit edebilirsiniz.

---

## **Avantajları**
- **Clean Code:** Kodunuz daha temiz ve okunabilir hale gelir.
- **SOLID Principles:** Hata yönetimi, SOLID prensiplerine uygun bir şekilde tasarlanmıştır.
- **Best Practices:** En iyi uygulamaları takip eder (örneğin, async/await, dependency injection).
- **Büyük Projelerde Kullanılabilirlik:** Modüler yapısı sayesinde büyük projelerde rahatlıkla kullanılabilir.

---

## **Kurulum ve Entegrasyon**

### **1. Projeye Ekleme**

#### **b. Manuel Ekleme**
Eğer bu kütüphaneyi manuel olarak ekliyorsanız, projenize **Core.CrossCuttingConcerns** klasörünü ekleyin ve gerekli referansları tanımlayın.

---

### **2. Program.cs veya Startup.cs Ayarları**
Kütüphaneyi kullanmak için `Program.cs` veya `Startup.cs` dosyasında aşağıdaki ayarları yapın:

#### **a. Middleware Ekleme**
Global hata yakalama middleware'ini ekleyin:

```csharp
using Core.CrossCuttingConcerns.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Diğer servislerin eklenmesi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware'lerin eklenmesi
app.UseExceptionMiddleware(); // Global hata yakalama middleware'i

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

#### **b. Loglama ve Diğer Servisler**
Loglama ve diğer servisleri ekleyin:

```csharp
builder.Services.AddLogging();
builder.Services.AddSingleton<IExceptionHandler, GlobalExceptionHandler>();
```

---

### **3. appsettings.json Ayarları**
Geliştirme ve üretim ortamları için farklı ayarlar yapabilirsiniz:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

---

## **Kullanım Örnekleri**

### **1. Özel Hata Türleri**
Özel hata türlerini kullanarak, belirli hataları fırlatabilirsiniz.

#### **NotFoundException Kullanımı:**
```csharp
public class ProductService
{
    public Product GetProductById(int id)
    {
        var product = _productRepository.GetById(id);
        if (product == null)
        {
            throw new NotFoundException("Product not found.");
        }
        return product;
    }
}
```

#### **ValidationException Kullanımı:**
```csharp
public class ProductService
{
    public void AddProduct(Product product)
    {
        if (product == null)
        {
            throw new ValidationException("Product cannot be null.");
        }

        // Ürün ekleme işlemi
    }
}
```

---

### **2. Global Hata Yakalama**
Global hata yakalama middleware'i, tüm hataları otomatik olarak yakalar ve işler.

#### **Örnek Hata Yanıtı:**
```json
{
  "success": false,
  "message": "Product not found.",
  "detail": null,
  "type": "NotFoundException",
  "errors": null,
  "stackTrace": "at ProductService.GetProductById(Int32 id) in ...",
  "errorCode": 404,
  "correlationId": "0HLQ9V9J1JQ9O",
  "timestamp": "2023-10-10T12:34:56Z"
}
```

---

### **3. Loglama**
Hatalar otomatik olarak loglanır. Örneğin, `ILogger` kullanılarak hatalar aşağıdaki gibi loglanır:

```plaintext
[12:34:56 ERR] An unhandled exception occurred. CorrelationId: 0HLQ9V9J1JQ9O
System.NotFoundException: Product not found.
   at ProductService.GetProductById(Int32 id) in ...
```

---

## **Tüm Exception Türleri ve Kullanım Örnekleri**

### **1. CustomException**
Genel amaçlı özel hatalar için kullanılır.

```csharp
throw new CustomException("An unexpected error occurred.");
```

### **2. ValidationException**
Validasyon hataları için kullanılır.

```csharp
var errors = new Dictionary<string, string[]>
{
    { "Email", new[] { "Email is required.", "Email must be a valid email address." } },
    { "Password", new[] { "Password must be at least 8 characters long." } }
};

throw new ValidationException(errors);
```

### **3. NotFoundException**
Kaynak bulunamadığında kullanılır.

```csharp
throw new NotFoundException("Product not found.");
```

### **4. UnauthorizedException**
Yetkisiz erişim durumlarında kullanılır.

```csharp
throw new UnauthorizedException("You are not authorized to access this resource.");
```

### **5. ConflictException**
Çakışma durumlarında kullanılır.

```csharp
throw new ConflictException("A conflict occurred while processing your request.");
```

### **6. ForbiddenException**
Yasaklı erişim durumlarında kullanılır.

```csharp
throw new ForbiddenException("Access to this resource is forbidden.");
```

---

## **Sonuç**
**Core.CrossCuttingConcerns** kütüphanesi, .NET Core uygulamalarında hata yönetimini kolaylaştıran ve standartlaştıran bir çözüm sunar. 
Bu kütüphaneyi kullanarak, hem geliştirme sürecinizi hızlandırabilir hem de daha tutarlı ve güvenilir bir uygulama oluşturabilirsiniz.