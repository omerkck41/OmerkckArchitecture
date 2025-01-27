# Core.ToolKit.UIHelpers

**Core.ToolKit.UIHelpers**, web uygulamalarında kullanılabilecek yardımcı araçlar içeren bir kütüphanedir. Bu kütüphane, hata sayfaları oluşturma ve bildirim mesajları yönetimi gibi işlemleri kolaylaştırmak için tasarlanmıştır. Büyük ölçekli projelerde kullanılmak üzere geliştirilmiştir ve temiz kod prensiplerine uygun olarak yazılmıştır.

---

## **Nedir?**

**Core.ToolKit.UIHelpers**, aşağıdaki işlevleri yerine getiren bir araç setidir:

1. **ErrorPageGenerator**: HTTP hata kodlarına özel HTML hata sayfaları oluşturur.
2. **NotificationHelper**: Başarı, hata ve uyarı mesajlarını standart bir şekilde yönetir ve loglar.

Bu kütüphane, özellikle web uygulamalarında kullanıcıya gösterilecek hata sayfalarını ve loglama işlemlerini merkezi bir şekilde yönetmek için kullanılır.

---

## **Neden Kullanılır?**

- **Hata Yönetimi**: HTTP hataları için özelleştirilmiş hata sayfaları oluşturur.
- **Loglama**: Uygulama içindeki başarı, hata ve uyarı mesajlarını standart bir şekilde loglar.
- **Esneklik**: HTML şablonları ve loglama hedefleri kolayca özelleştirilebilir.
- **Temiz Kod**: Best practice'lere uygun olarak yazılmıştır ve büyük projelerde rahatça kullanılabilir.

---

## **Avantajları**

- **Kolay Entegrasyon**: Mevcut projelere kolayca entegre edilebilir.
- **Genişletilebilirlik**: Yeni özellikler eklenebilir veya mevcut özellikler özelleştirilebilir.
- **Asenkron Destek**: Tüm metotlar asenkron olarak çalışır, bu da performansı artırır.
- **Loglama Esnekliği**: Logları konsola, dosyaya veya harici bir servise gönderme imkanı sunar.

---

## **Kurulum ve Ayarlar**

### 2. **Program.cs ve AppSettings.json Ayarları**

#### **Program.cs**

Kütüphaneyi kullanmak için `Program.cs` dosyasında gerekli ayarlamaları yapın. Örneğin, hata sayfalarını ve loglama işlemlerini yapılandırabilirsiniz.

```csharp
using Core.ToolKit.UIHelpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// NotificationHelper için loglama servisini ekleyin (isteğe bağlı)
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole(); // Konsola loglama
    loggingBuilder.AddDebug();   // Debug penceresine loglama
});

var app = builder.Build();

// Hata sayfası middleware'i (örnek)
app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode >= 400)
    {
        var errorMessage = "Bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
        var errorPage = await ErrorPageGenerator.GenerateAsync(context.Response.StatusCode, errorMessage);
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(errorPage);
    }
});

app.Run();
```

#### **AppSettings.json**

Loglama ve hata sayfaları için gerekli ayarları `appsettings.json` dosyasında yapılandırabilirsiniz.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ErrorPageSettings": {
    "CustomCss": "<style>body { font-family: Arial, sans-serif; text-align: center; padding: 50px; } h1 { font-size: 50px; color: red; } p { font-size: 20px; }</style>"
  }
}
```

---

## **Detaylı Kullanım Örnekleri**

### 1. **ErrorPageGenerator Kullanımı**

HTTP hataları için özelleştirilmiş hata sayfaları oluşturmak için `ErrorPageGenerator` sınıfını kullanabilirsiniz.

```csharp
using Core.ToolKit.UIHelpers;

public async Task<string> GenerateErrorPageAsync(int statusCode, string errorMessage)
{
    var customCss = "<style>body { font-family: Arial, sans-serif; text-align: center; padding: 50px; } h1 { font-size: 50px; color: red; } p { font-size: 20px; }</style>";
    return await ErrorPageGenerator.GenerateAsync(statusCode, errorMessage, customCss);
}
```

### 2. **NotificationHelper Kullanımı**

Başarı, hata ve uyarı mesajlarını loglamak için `NotificationHelper` sınıfını kullanabilirsiniz.

```csharp
using Core.ToolKit.UIHelpers;

public async Task LogMessagesAsync()
{
    await NotificationHelper.LogAsync("Uygulama başlatıldı.", LogLevel.Info);

    try
    {
        // Bir işlem yap
        await NotificationHelper.LogAsync("İşlem başarılı.", LogLevel.Success);
    }
    catch (Exception ex)
    {
        await NotificationHelper.LogAsync("İşlem sırasında bir hata oluştu.", LogLevel.Error, ex.ToString());
    }
}
```

### 3. **Middleware ile Hata Sayfaları**

Middleware kullanarak tüm HTTP hataları için otomatik hata sayfaları oluşturabilirsiniz.

```csharp
app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode >= 400)
    {
        var errorMessage = "Bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
        var errorPage = await ErrorPageGenerator.GenerateAsync(context.Response.StatusCode, errorMessage);
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(errorPage);
    }
});
```

---

## **Sonuç**

**Core.ToolKit.UIHelpers**, web uygulamalarında hata yönetimi ve loglama işlemlerini kolaylaştıran güçlü bir araç setidir. Temiz kod prensiplerine uygun olarak yazılmıştır ve büyük projelerde rahatça kullanılabilir. Yukarıdaki örnekler ve ayarlarla projenize entegre edebilirsiniz.