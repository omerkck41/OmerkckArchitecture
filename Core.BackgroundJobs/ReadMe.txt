# Core.BackgroundJobs - Kullanım ve Entegrasyon Rehberi

## **Nedir?**
Core.BackgroundJobs modülü, Hangfire ve Quartz.NET gibi güçlü kütüphaneleri kullanarak arka planda çalışacak görevlerin planlanması ve yönetilmesi için tasarlanmış bir altyapıdır. Zamanlanmış, gecikmeli veya kuyruk tabanlı işler bu modül ile kolayca uygulanabilir.

---

## **Neden Kullanılır?**
Core.BackgroundJobs modülü, uygulamalarınızda zamanlanmış görevlerin ve uzun süren işlemlerin güvenilir bir şekilde arka planda çalışmasını sağlar. Örneğin:
- Belirli zamanlarda otomatik rapor oluşturma.
- Kullanıcılara hatırlatma e-postaları gönderme.
- Veritabanı temizlik işlemlerini belirli aralıklarla çalıştırma.
- Yoğun iş yükünü kullanıcı etkileşiminden bağımsız olarak arka planda gerçekleştirme.

---

## **Avantajları**
1. **Modüler Yapı**:
   - Hem **Hangfire** hem de **Quartz.NET** entegrasyon desteği.
   - İhtiyaca göre uygun kütüphane seçimi.

2. **Esnek Görev Planlama**:
   - Zamanlanmış (Cron tabanlı) görevler.
   - Gecikmeli ve anında çalıştırılabilir işler.

3. **Kolay Entegrasyon**:
   - Program.cs ve appsettings.json üzerinden hızlı yapılandırma.

4. **Gelişmiş Yönetim**:
   - Hangfire Dashboard gibi görsel araçlarla işlerin durumu ve geçmişi takip edilebilir.

5. **Çoklu Veritabanı Desteği**:
   - SQL Server ve MySQL gibi farklı veritabanı seçenekleri ile uyumluluk.

---

## **Projeye Entegrasyon**

### **1. Gerekli NuGet Paketlerinin Kurulumu**
Projenize aşağıdaki NuGet paketlerini ekleyin:

#### Hangfire için:
```bash
Install-Package Hangfire
Install-Package Hangfire.SqlServer
Install-Package Hangfire.MySql
```

#### Quartz.NET için:
```bash
Install-Package Quartz
```

---

### **2. Program.cs Yapılandırması**
Core.BackgroundJobs modülünü projenize eklemek için aşağıdaki servis kayıtlarını kullanın:

#### **Hangfire Ayarları**:
```csharp
using Core.BackgroundJobs.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Hangfire Servis Yapılandırması
builder.Services.AddHangfireJobs(builder.Configuration);
```

#### **Quartz.NET Ayarları**:
```csharp
// Quartz.NET Servis Yapılandırması
builder.Services.AddQuartzJobs();

var app = builder.Build();

// Örnek İşlerin Planlanması
var jobScheduler = app.Services.GetRequiredService<IJobScheduler>();
jobScheduler.ScheduleRecurringJob<EmailReminderJob>("email-reminder-job", Cron.Daily);
jobScheduler.ScheduleDelayedJob<DatabaseCleanupJob>("database-cleanup-job", TimeSpan.FromMinutes(5));

app.Run();
```

---

### **3. appsettings.json Yapılandırması**
`appsettings.json` dosyasına Hangfire için gerekli bağlantı ayarlarını ekleyin:

```json
{
  "Hangfire": {
    "StorageType": "MySql", // SqlServer veya MySql olarak seçilebilir
    "ConnectionStrings": {
      "HangfireConnection": "Server=localhost;Database=hangfire_db;User=root;Password=yourpassword;"
    }
  }
}
```

- **StorageType**: Veritabanı türünü seçin (`SqlServer` veya `MySql`).
- **ConnectionStrings**: Kullanmak istediğiniz veritabanının bağlantı bilgilerini ekleyin.

---

## **Detaylı Kullanım Örnekleri**

### **1. Zamanlanmış Görev Oluşturma**
Belirli aralıklarla çalışacak görevler için:
```csharp
var jobScheduler = app.Services.GetRequiredService<IJobScheduler>();
jobScheduler.ScheduleRecurringJob<EmailReminderJob>("daily-email-job", Cron.Daily);
```
> Bu örnekte, **EmailReminderJob** her gün bir kez çalışır.

---

### **2. Gecikmeli Görev Planlama**
Gecikmeli bir görevi çalıştırmak için:
```csharp
jobScheduler.ScheduleDelayedJob<DatabaseCleanupJob>("cleanup-job", TimeSpan.FromMinutes(10));
```
> **DatabaseCleanupJob** 10 dakika sonra çalışır.

---

### **3. Anında Görev Ekleme**
Kuyruğa bir iş eklemek için:
```csharp
jobScheduler.EnqueueJob<DatabaseCleanupJob>();
```
> İş, anında kuyruğa eklenir ve en kısa sürede çalıştırılır.

---

### **4. Quartz.NET Kullanımı**
Quartz.NET ile zamanlanmış görevlerin yapılandırması:
```csharp
builder.Services.AddQuartzJobs();

var quartzScheduler = app.Services.GetRequiredService<IJobScheduler>();
quartzScheduler.ScheduleRecurringJob<EmailReminderJob>("weekly-email-job", "0 0 12 ? * MON"); // Her Pazartesi 12:00'de
```
> Quartz.NET, **CRON** ifadeleri ile esnek zamanlama sağlar.

---

## **Sonuç**
Core.BackgroundJobs modülü, arka planda çalışan işleri yönetmek için modern ve güçlü bir çözüm sunar. Hem **Hangfire** hem de **Quartz.NET** desteği sayesinde ihtiyacınıza göre uygun planlama mekanizmasını kullanabilirsiniz. Yapılandırması kolay, genişletilebilir ve büyük ölçekli projelere uygun bir altyapı sağlar.
