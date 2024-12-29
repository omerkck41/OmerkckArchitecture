# Core.Application.Logging - README

**Core.Application.Logging**, modern yazılım projelerinde gelişmiş ve esnek loglama mekanizmaları sunar. Bu yapı hem **Behavior** tabanlı MediatR pipeline loglamayı hem de bağımsız **Service** tabanlı loglama çözümlerini destekler. Projenizde loglama ihtiyaçlarını karşılamak ve temiz kod yazmayı kolaylaştırmak için tasarlanmıştır.

---

## **1. Özellikler**

1. **Behavior Tabanlı Loglama**:
   - MediatR pipeline entegrasyonu ile request ve response loglaması.
   - Hataları yakalama ve detaylı hata raporlaması.

2. **Service Tabanlı Loglama**:
   - Doğrudan kullanılabilir bağımsız bir loglama servisi.
   - Farklı log seviyeleri (Info, Warning, Error, Debug).

3. **Esneklik ve Genişletilebilirlik**:
   - Dosya, konsol ve veri tabanı gibi farklı log hedeflerini destekler.
   - Konfigürasyonlar appsettings.json dosyasından yönetilebilir.

---

## **2. Projeye Entegrasyon**

### 2.1 NuGet Paketleri

Gerekli paketleri yükleyin:

```bash
Install-Package MediatR.Extensions.Microsoft.DependencyInjection
Install-Package Microsoft.Extensions.Logging
Install-Package Serilog.Extensions.Logging
Install-Package Serilog
Install-Package Serilog.Sinks.File
```

---

### 2.2 Program.cs Ayarları

**Dependency Injection** yapılandırmasını ekleyin:

```csharp
using Core.Application.Logging;

var builder = WebApplication.CreateBuilder(args);

// Logging configurations
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddFile("logs/log-.txt", rollingInterval: RollingInterval.Day);
});

// Add MediatR pipeline behavior
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

// Add Logging Service
builder.Services.AddScoped<ILoggingService, LoggingService>();

var app = builder.Build();

app.Run();
```

---

## **3. Kullanım**

### **3.1 Behavior Tabanlı Kullanım**

#### 3.1.1 Request Sınıfı
Behavior tabanlı loglama için, loglanacak request’lerin `ILoggableRequest` arayüzünü implement etmesi gerekmektedir.

```csharp
using MediatR;
using Core.Application.Logging;

public class SampleRequest : IRequest<string>, ILoggableRequest
{
    public string Message { get; set; }
}
```

#### 3.1.2 Request Handler
Handler, request’in işlenmesi sırasında çalışır ve loglama işlemi pipeline tarafından otomatik yapılır.

```csharp
using MediatR;

public class SampleRequestHandler : IRequestHandler<SampleRequest, string>
{
    public Task<string> Handle(SampleRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Handled Message: {request.Message}");
    }
}
```

#### 3.1.3 Örnek Kullanım

```csharp
var mediator = serviceProvider.GetRequiredService<IMediator>();
var response = await mediator.Send(new SampleRequest { Message = "Hello, World!" });
```

---

### **3.2 Service Tabanlı Kullanım**

#### 3.2.1 Logging Service Kullanımı
`ILoggingService` ile bağımsız olarak loglama işlemleri yapılabilir.

```csharp
using Core.Application.Logging;

public class SomeService
{
    private readonly ILoggingService _loggingService;

    public SomeService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public void PerformOperation()
    {
        try
        {
            _loggingService.LogInfo("Starting operation...");

            // İşlem kodları
            _loggingService.LogDebug("Operation is running.");

            // Hata durumunda
            throw new InvalidOperationException("Something went wrong!");
        }
        catch (Exception ex)
        {
            _loggingService.LogError("An error occurred during the operation.", ex);
        }
    }
}
```

---

## **4. appsettings.json Konfigürasyonu**

Loglama hedeflerini ve seviyelerini `appsettings.json` dosyasından yönetebilirsiniz:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Core.Application": "Debug"
    },
    "File": {
      "Path": "logs/log-.txt",
      "RollingInterval": "Day"
    }
  }
}
```

---

## **5. Özelliklerin Karşılaştırması**

| **Özellik**        | **Behavior Tabanlı**                    | **Service Tabanlı**                   |
|--------------------|-----------------------------------------|---------------------------------------|
| **Kapsam**         | MediatR pipeline kullanımlarında        | Bağımsız loglama işlemleri için       |
| **Otomasyon**      | Request ve response otomatik loglanır  | Manuel loglama yapılır               |
| **Kullanım Kolaylığı** | MediatR kullanan projeler için kolay   | Her türlü proje için esnek            |

---

## **6. Sonuç**

**Core.Application.Logging**, hem Behavior tabanlı hem de bağımsız loglama ihtiyaçlarını karşılayabilecek esnek ve genişletilebilir bir yapıdır. Projelerinizdeki loglama işlemlerini düzenlemek, performansı artırmak ve hata ayıklamayı kolaylaştırmak için bu yapıyı kullanabilirsiniz.