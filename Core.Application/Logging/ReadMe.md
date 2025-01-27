# LoggingService ve LoggingBehavior Kullanımı

## Amaç
Bu proje, uygulamanızdaki loglama süreçlerini standartlaştırmak ve loglama işlemlerini bir hizmet katmanına taşımak için geliştirilmiştir. Aynı zamanda, logların farklı seviyelerde (Info, Debug, Warning, Error, Trace) kaydedilmesi ve external sink’lere (örneğin, Elasticsearch) gönderilmesi desteklenir.

## Özellikler
- Merkezi loglama servisi (`LoggingService`) ile logların düzenli bir şekilde yönetimi.
- MediatR pipeline behavior ile otomatik loglama desteği.
- JSON formatında log verisi desteği.
- Elasticsearch gibi external sink entegrasyonu.
- Farklı log seviyeleri: Trace, Debug, Info, Warning, Error.
- EventId desteğiyle detaylı loglama.

---

## Kullanılan Teknolojiler
- **.NET 9.0**
- **MediatR**: Request pipeline oluşturmak için.
- **Microsoft.Extensions.Logging**: Standart loglama işlemleri için.
- **Serilog**: Elasticsearch entegrasyonu için.
- **Newtonsoft.Json**: Logları JSON formatına dönüştürmek için.

---

## Projeye Entegrasyon

### 1. NuGet Paketlerini Yükleme
Aşağıdaki NuGet paketlerini yükleyerek başlayın:
```bash
# MediatR ve Serilog paketleri
 dotnet add package MediatR.Extensions.Microsoft.DependencyInjection
 dotnet add package Serilog
 dotnet add package Serilog.Sinks.Elasticsearch
 dotnet add package Microsoft.Extensions.Logging
 dotnet add package Newtonsoft.Json
```

---

### 2. appsettings.json Ayarları
Loglama yapılandırması için aşağıdaki gibi bir konfigürasyon ekleyin:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "System": "Warning",
      "Core.Application.Logging": "Trace"
    },
    "Elasticsearch": {
      "Url": "http://localhost:9200",
      "IndexFormat": "app-logs-{0:yyyy.MM.dd}"
    }
  }
}
```

---

### 3. Program.cs
`LoggingService` ve `LoggingBehavior` yapılandırmasını ekleyin:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Core.Application.Logging.Behaviors;
using Core.Application.Logging.Services;
using MediatR;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(configuration["Logging:Elasticsearch:Url"]))
    {
        AutoRegisterTemplate = true,
        IndexFormat = configuration["Logging:Elasticsearch:IndexFormat"]
    })
    .CreateLogger();

var builder = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureServices((context, services) =>
    {
        services.AddLoggingServices(); // LoggingService ve LoggingBehavior kaydı
        services.AddMediatR(typeof(Program));
    });

await builder.Build().RunAsync();
```

---

### 4. Dependency Injection (ServiceCollectionExtensions)
`ServiceCollectionExtensions` sınıfı:

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLoggingServices(this IServiceCollection services)
    {
        services.AddScoped<ILoggingService, LoggingService>();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        return services;
    }
}
```

---

## Kullanım Örnekleri

### 1. ILoggableRequest Arayüzü Kullanan Bir Request
```csharp
using Core.Application.Logging.Behaviors;

public class CreateOrderRequest : ILoggableRequest
{
    public string OrderId { get; set; }
    public string CustomerName { get; set; }
    public decimal Amount { get; set; }

    public string RequestDetails => $"OrderId: {OrderId}, Customer: {CustomerName}, Amount: {Amount}";
    public Dictionary<string, object>? Metadata => new Dictionary<string, object>
    {
        { "Timestamp", DateTime.UtcNow }
    };
}
```

### 2. Handler İçinde LoggingService Kullanımı
```csharp
using Core.Application.Logging.Services;
using MediatR;

public class CreateOrderHandler : IRequestHandler<CreateOrderRequest, bool>
{
    private readonly ILoggingService _loggingService;

    public CreateOrderHandler(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public async Task<bool> Handle(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _loggingService.LogInfo("Order creation started", request);

            // İşlem kodları buraya

            _loggingService.LogInfo("Order successfully created", request);
            return true;
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Order creation failed", ex, request);
            throw;
        }
    }
}
```

### 3. Trace Loglama Örneği
```csharp
_loggingService.LogTrace("Debugging details", new { Key = "Value", Status = "InProgress" });
```

---