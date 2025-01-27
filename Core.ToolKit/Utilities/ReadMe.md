# Core.ToolKit.Utilities

**Core.ToolKit.Utilities**, .NET projelerinde sıkça kullanılan yardımcı araçlar ve işlevler içeren bir kütüphanedir. Bu kütüphane, zaman yönetimi, olay yönetimi, arka plan işlemleri, hata yönetimi ve performans ölçümü gibi işlevleri kolayca entegre etmek için tasarlanmıştır. Büyük ölçekli projelerde kullanılabilecek şekilde esnek ve genişletilebilir bir yapıya sahiptir.

---

## **Nedir?**

**Core.ToolKit.Utilities**, .NET geliştiricileri için hazırlanmış bir yardımcı kütüphanedir. Bu kütüphane, geliştiricilerin sıkça ihtiyaç duyduğu ancak her projede tekrar tekrar yazmak zorunda kaldığı işlevleri merkezi bir yapıda sunar. Özellikle zaman yönetimi, olay yönetimi, arka plan işlemleri ve hata yönetimi gibi konularda geliştiricilere kolaylık sağlar.

---

## **Neden Kullanılır?**

- **Tekrar Kullanılabilirlik**: Sıkça kullanılan işlevler tek bir kütüphanede toplanarak kod tekrarı önlenir.
- **Esneklik**: Büyük ölçekli projelerde kullanılabilecek şekilde tasarlanmıştır.
- **Performans**: Asenkron işlemler ve performans ölçümü gibi işlevlerle uygulamanın performansını artırır.
- **Hata Yönetimi**: Üstel geri çekme (exponential backoff) ve detaylı hata yönetimi ile güvenilir bir yapı sunar.
- **Zaman Yönetimi**: Zaman aralıklarını yönetmek ve bölmek için kullanışlı metotlar içerir.

---

## **Avantajları**

- **Clean Code**: Temiz ve anlaşılır bir kod yapısına sahiptir.
- **Async Yapı**: Tüm metotlar asenkron olarak tasarlanmıştır.
- **Genişletilebilirlik**: Yeni metotlar ve özellikler eklenerek kolayca genişletilebilir.
- **Büyük Proje Desteği**: Büyük ölçekli projelerde rahatça kullanılabilir.
- **Detaylı Hata Yönetimi**: Üstel geri çekme ve detaylı hata yönetimi ile güvenilir bir yapı sunar.

---

## **Kurulum ve Projeye Ekleme**

#### Manuel Kurulum:
- `Core.ToolKit.Utilities.dll` dosyasını projenize referans olarak ekleyin.

---

### 2. **Ayarlar ve Yapılandırma**

#### **Program.cs**
Kütüphaneyi kullanmak için `Program.cs` dosyasında gerekli ayarlamaları yapın. Örneğin, arka plan işlemleri veya olay dinleyicileri ekleyebilirsiniz.

```csharp
using Core.ToolKit.Utilities;
using Core.ToolKit.Utilities.EventManagement;

class Program
{
    static async Task Main(string[] args)
    {
        // Olay dinleyicisi ekleme
        ToolkitEvents.SubscribeProcessStarted((sender, e) =>
        {
            Console.WriteLine($"Process Started: {e.Message}");
        });

        ToolkitEvents.SubscribeProcessCompleted((sender, e) =>
        {
            Console.WriteLine($"Process Completed: {e.Message}");
        });

        // Arka plan işlemi örneği
        await BackgroundWorkerAlert.RunWithProgressAsync(async (progress) =>
        {
            for (int i = 0; i <= 100; i += 10)
            {
                progress.Report(i);
                await Task.Delay(100);
            }
            return "Task Completed";
        }, 
        onProgress: (progress) => Console.WriteLine($"Progress: {progress}%"),
        onSuccess: (result) => Console.WriteLine(result),
        onError: (ex) => Console.WriteLine($"Error: {ex.Message}"));

        // Zaman aralığı örneği
        var ranges = TimeRangeHelper.SplitRangeWithMetadata(DateTime.Now, DateTime.Now.AddHours(2), 30);
        foreach (var range in ranges)
        {
            Console.WriteLine($"Range: {range.Start} - {range.End}, Index: {range.IntervalIndex}");
        }
    }
}
```

#### **appsettings.json**
Eğer kütüphane ile ilgili yapılandırma ayarları gerekiyorsa, `appsettings.json` dosyasına ekleyebilirsiniz. Örneğin, üstel geri çekme ayarları:

```json
{
  "RetrySettings": {
    "MaxRetryCount": 5,
    "InitialDelayMilliseconds": 1000,
    "MaxDelayMilliseconds": 10000
  }
}
```

---

## **Detaylı Kullanım Örnekleri**

### 1. **Olay Yönetimi (Event Management)**
`ToolkitEvents` sınıfı ile olayları dinleyebilir ve tetikleyebilirsiniz.

```csharp
// Olay dinleyicisi ekleme
ToolkitEvents.SubscribeProcessStarted((sender, e) =>
{
    Console.WriteLine($"Process Started: {e.Message}");
});

// Olay tetikleme
ToolkitEvents.TriggerProcessStarted(this, "Process 1 Started");
```

---

### 2. **Arka Plan İşlemleri (Background Worker)**
`BackgroundWorkerAlert` sınıfı ile arka planda çalışacak işlemleri yönetebilirsiniz.

```csharp
await BackgroundWorkerAlert.RunWithProgressAsync(async (progress) =>
{
    for (int i = 0; i <= 100; i += 10)
    {
        progress.Report(i);
        await Task.Delay(100);
    }
    return "Task Completed";
}, 
onProgress: (progress) => Console.WriteLine($"Progress: {progress}%"),
onSuccess: (result) => Console.WriteLine(result),
onError: (ex) => Console.WriteLine($"Error: {ex.Message}"));
```

---

### 3. **Zaman Aralığı Yönetimi (Time Range Management)**
`TimeRangeHelper` sınıfı ile zaman aralıklarını yönetebilirsiniz.

```csharp
// Zaman aralıklarını bölme
var ranges = TimeRangeHelper.SplitRangeWithMetadata(DateTime.Now, DateTime.Now.AddHours(2), 30);
foreach (var range in ranges)
{
    Console.WriteLine($"Range: {range.Start} - {range.End}, Index: {range.IntervalIndex}");
}

// Zaman aralıklarının çakışıp çakışmadığını kontrol etme
bool isOverlap = TimeRangeHelper.DoRangesOverlap(
    DateTime.Now, DateTime.Now.AddHours(1),
    DateTime.Now.AddMinutes(30), DateTime.Now.AddHours(2)
);
Console.WriteLine($"Ranges Overlap: {isOverlap}");
```

---

### 4. **Üstel Geri Çekme ile Yeniden Deneme (Retry with Exponential Backoff)**
`UtilityHelper` sınıfı ile üstel geri çekme mekanizmasını kullanabilirsiniz.

```csharp
var result = await UtilityHelper.RetryWithExponentialBackoffAsync(async () =>
{
    // Burada yeniden denenmesi gereken işlem yapılır
    return await SomeOperation();
}, retryCount: 3, initialDelayMilliseconds: 1000);
```

---

### 5. **Performans Ölçümü (Performance Measurement)**
`UtilityHelper` sınıfı ile bir işlemin çalışma süresini ölçebilirsiniz.

```csharp
var (executionTime, result) = await UtilityHelper.MeasureExecutionTimeAsync(async () =>
{
    await Task.Delay(1000); // Simüle edilmiş bir işlem
    return "Operation Completed";
});
Console.WriteLine($"Execution Time: {executionTime} ms, Result: {result}");
```

---

## **Sonuç**

**Core.ToolKit.Utilities**, .NET projelerinde sıkça kullanılan işlevleri merkezi bir yapıda sunarak geliştiricilere büyük kolaylık sağlar. Büyük ölçekli projelerde rahatça kullanılabilecek şekilde tasarlanmıştır ve genişletilebilir bir yapıya sahiptir. Bu kütüphaneyi kullanarak kod tekrarını azaltabilir, uygulamanızın performansını artırabilir ve daha temiz bir kod yapısına sahip olabilirsiniz.