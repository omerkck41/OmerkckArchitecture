### **Core.BackgroundJobs - Readme**

---

## **Core.BackgroundJobs Nedir?**
`Core.BackgroundJobs`, .NET uygulamalarında arka plan işlerini (background jobs) yönetmek için geliştirilmiş bir kütüphanedir. Bu kütüphane, **Hangfire** ve **Quartz.NET** gibi popüler arka plan işlem kütüphanelerini entegre ederek, tekrarlayan (recurring), gecikmeli (delayed) ve anında (immediate) işlerin kolayca planlanmasını ve yönetilmesini sağlar.

Bu proje, modern .NET uygulamalarında arka plan işlerini yönetmek için esnek ve genişletilebilir bir çözüm sunar.

---

## **Neler Kullanıldı?**
- **Hangfire**: Arka plan işlerini yönetmek için kullanılan popüler bir kütüphane. SQL Server veya MySQL gibi veritabanları üzerinde işlerin durumunu takip eder.
- **Quartz.NET**: Zamanlanmış işler (scheduled jobs) için kullanılan bir kütüphane. Cron tabanlı zamanlama desteği sunar.
- **Dependency Injection (DI)**: Servislerin bağımlılıklarını yönetmek için .NET Core'un yerleşik DI mekanizması kullanıldı.
- **Logging**: İşlemlerin izlenmesi ve hata ayıklama için `ILogger` mekanizması entegre edildi.

---

## **Ne İşe Yarar?**
- **Tekrarlayan İşler**: Belirli bir zaman aralığında (cron ifadesi ile) tekrarlanması gereken işleri planlar (örneğin, her gün saat 10:00'da veritabanı temizleme).
- **Gecikmeli İşler**: Belirli bir süre sonra çalıştırılacak işleri planlar (örneğin, 5 dakika sonra e-posta gönderme).
- **Anında İşler**: Hemen çalıştırılması gereken işleri kuyruğa alır (örneğin, kullanıcı kaydı tamamlandığında hoş geldin e-postası gönderme).

---

## **Projeye Nasıl Eklenir?**

### 1. **Projeye Paketlerin Eklenmesi**
Projenize aşağıdaki NuGet paketlerini ekleyin:

```bash
dotnet add package Hangfire
dotnet add package Hangfire.MySqlStorage
dotnet add package Quartz
dotnet add package Microsoft.Extensions.Configuration
dotnet add package Microsoft.Extensions.DependencyInjection
dotnet add package Microsoft.Extensions.Logging
```

---

### 2. **Program.cs Ayarları**
`Program.cs` dosyasında `Core.BackgroundJobs` kütüphanesini kullanmak için gerekli servisleri ekleyin.

#### Örnek:
```csharp
using Core.BackgroundJobs.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Configuration dosyasını yükle
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Hangfire veya Quartz servislerini ekle
builder.Services.AddHangfireJobs(builder.Configuration); // Hangfire kullanmak için
// builder.Services.AddQuartzJobs(); // Quartz kullanmak için

var host = builder.Build();
host.Run();
```

---

### 3. **appsettings.json Ayarları**
`appsettings.json` dosyasında Hangfire ve Quartz için gerekli yapılandırmaları ekleyin.

#### Örnek:
```json
{
  "ConnectionStrings": {
    "HangfireConnection": "Server=your_server;Database=your_database;User Id=your_user;Password=your_password;"
  },
  "Hangfire": {
    "StorageType": "MySql" // veya "SqlServer"
  }
}
```

---

## **Kullanım Örnekleri**

### 1. **Tekrarlayan İş Planlama (Recurring Job)**
Her gün saat 10:00'da çalışacak bir iş planlamak için:

```csharp
public class DatabaseCleanupJob : IJob
{
    private readonly ILogger<DatabaseCleanupJob> _logger;

    public DatabaseCleanupJob(ILogger<DatabaseCleanupJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Database cleanup job started.");
        // Veritabanı temizleme işlemleri
        await Task.Delay(1000); // Simüle edilmiş işlem
        _logger.LogInformation("Database cleanup job completed.");
    }
}

// Program.cs veya başka bir yerde işi planlama
var jobScheduler = host.Services.GetRequiredService<IJobScheduler>();
await jobScheduler.ScheduleRecurringJob<DatabaseCleanupJob>("DatabaseCleanup", "0 0 10 * * ?");
```

---

### 2. **Gecikmeli İş Planlama (Delayed Job)**
5 dakika sonra çalışacak bir iş planlamak için:

```csharp
public class EmailReminderJob : IJob
{
    private readonly ILogger<EmailReminderJob> _logger;

    public EmailReminderJob(ILogger<EmailReminderJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Sending email reminder...");
        // E-posta gönderme işlemleri
        await Task.Delay(1000); // Simüle edilmiş işlem
        _logger.LogInformation("Email reminder sent.");
    }
}

// Program.cs veya başka bir yerde işi planlama
var jobScheduler = host.Services.GetRequiredService<IJobScheduler>();
await jobScheduler.ScheduleDelayedJob<EmailReminderJob>("EmailReminder", TimeSpan.FromMinutes(5));
```

---

### 3. **Anında İş Kuyruğa Alma (Enqueue Job)**
Hemen çalıştırılacak bir işi kuyruğa almak için:

```csharp
public class SampleJob : IJob
{
    private readonly ILogger<SampleJob> _logger;

    public SampleJob(ILogger<SampleJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Sample job started.");
        await Task.Delay(1000); // Simüle edilmiş işlem
        _logger.LogInformation("Sample job completed.");
    }
}

// Program.cs veya başka bir yerde işi kuyruğa alma
var jobScheduler = host.Services.GetRequiredService<IJobScheduler>();
await jobScheduler.EnqueueJob<SampleJob>();
```

---

## **Örnek Proje Yapısı**
```
Core.BackgroundJobs/
├── Jobs/
│   ├── DatabaseCleanupJob.cs
│   ├── EmailReminderJob.cs
│   └── SampleJob.cs
├── Services/
│   ├── HangfireJobScheduler.cs
│   └── QuartzJobScheduler.cs
├── Interfaces/
│   └── IJobScheduler.cs
├── Extensions/
│   └── BackgroundJobExtensions.cs
└── Core.BackgroundJobs.csproj
```

---

## **Sonuç**
`Core.BackgroundJobs`, .NET uygulamalarında arka plan işlerini yönetmek için esnek ve genişletilebilir bir çözüm sunar. Hem **Hangfire** hem de **Quartz.NET** entegrasyonu sayesinde, farklı ihtiyaçlara uygun çözümler sunar. Bu kütüphaneyi kullanarak, tekrarlayan, gecikmeli ve anında işlerinizi kolayca planlayabilir ve yönetebilirsiniz. 🚀