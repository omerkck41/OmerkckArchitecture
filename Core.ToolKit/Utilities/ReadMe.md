# Core.Toolkit Utilities Module

**Utilities Module**, büyük ölçekli projelerde arka plan görevleri, zaman aralıkları ve genel yardımcı işlevlerle ilgili işlemleri kolaylaştırmak için optimize edilmiş araçlar sunar.

---

## **1. BackgroundWorkerAlert**

### **Açıklama**
Arka plan görevlerini çalıştırır ve gelişmiş ilerleme takibi, hata işleme desteği sağlar.

### **Kullanım**

#### **Arka Plan Görevini Çalıştırma**
```csharp
using Core.Toolkit.Utilities;

await BackgroundWorkerAlert.RunWithProgressAsync(
    async progress =>
    {
        for (int i = 0; i <= 100; i += 10)
        {
            await Task.Delay(100);
            progress.Report(i);
        }
        return "Task Completed";
    },
    onProgress: value => Console.WriteLine($"Progress: {value}%"),
    onSuccess: result => Console.WriteLine(result),
    onError: ex => Console.WriteLine($"Error: {ex.Message}")
);
```

---

## **2. UtilityHelper**

### **Açıklama**
Görevleri yeniden deneme ve çalışma süresi ölçümü gibi genel yardımcı işlevler sağlar.

### **Kullanım**

#### **Görevleri Exponential Backoff ile Yeniden Deneme**
```csharp
using Core.Toolkit.Utilities;

var result = await UtilityHelper.RetryWithExponentialBackoffAsync(
    async () =>
    {
        // İşlemi burada tanımlayın
        return await SomeUnreliableServiceCall();
    },
    retryCount: 5,
    initialDelayMilliseconds: 500
);
```

#### **Çalışma Süresi Ölçümü**
```csharp
using Core.Toolkit.Utilities;

var (executionTime, result) = await UtilityHelper.MeasureExecutionTimeAsync(async () =>
{
    await Task.Delay(2000); // Örnek işlem
    return "Completed";
});

Console.WriteLine($"Execution Time: {executionTime} ms, Result: {result}");
```

---

## **3. TimeRangeHelper**

### **Açıklama**
Zaman aralıklarını yönetmek ve işlem yapmak için araçlar sağlar.

### **Kullanım**

#### **Zaman Aralıklarının Çakışmasını Kontrol Etme**
```csharp
using Core.Toolkit.Utilities;

bool overlaps = TimeRangeHelper.DoRangesOverlap(
    new DateTime(2024, 12, 1, 9, 0, 0),
    new DateTime(2024, 12, 1, 10, 0, 0),
    new DateTime(2024, 12, 1, 9, 30, 0),
    new DateTime(2024, 12, 1, 10, 30, 0)
);

Console.WriteLine(overlaps); // Output: True
```

#### **Zaman Aralığını Bölmek**
```csharp
using Core.Toolkit.Utilities;

var intervals = TimeRangeHelper.SplitRangeWithMetadata(
    new DateTime(2024, 12, 1, 9, 0, 0),
    new DateTime(2024, 12, 1, 12, 0, 0),
    30
);

foreach (var interval in intervals)
{
    Console.WriteLine($"Start: {interval.Start}, End: {interval.End}, Index: {interval.IntervalIndex}");
}
```

---

## **4. ToolkitEvents**

### **Açıklama**
Standart olay yönetimi için araçlar sağlar.

### **Kullanım**

#### **Bir Olay Tetikleme**
```csharp
using Core.Toolkit.Utilities;

ToolkitEvents.ProcessStarted += (sender, args) =>
{
    Console.WriteLine($"Process Started: {args.Message}");
};

ToolkitEvents.TriggerProcessStarted("My Background Task");

ToolkitEvents.ProcessCompleted += (sender, args) =>
{
    Console.WriteLine($"Process Completed: {args.Message}");
};

ToolkitEvents.TriggerProcessCompleted("My Background Task Completed");
```

---

## **Özellikler ve Avantajlar**

- **Arka Plan Görevleri**: Büyük ölçekli işlemleri kolayca yönetme.
- **Zaman Aralığı İşlemleri**: Kesişme kontrolü ve aralık bölme desteği.
- **Genel Yardımcılar**: Görevleri yeniden deneme, çalışma süresini ölçme.
- **Olay Yönetimi**: Standart olay yönetimi ve tetikleme.
- **Optimize Performans**: Büyük veri setleri ve yüksek kullanıcı yüküne uygun.

---
