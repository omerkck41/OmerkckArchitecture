# Core.Application.Excel Katmanı - ReadMe Dosyası

## **Nedir?**
**Core.Application.Excel** katmanı, Excel dosyaları ile programatik olarak etkileşim kurmayı sağlayan bir **kütüphane**dir. Excel dosyalarını oluşturma, okuma, yazma, düzenleme ve PDF formatında dışa aktarma gibi işlemleri destekler. Asenkron çalışma yapısıyla performansı artırır ve büyük veri setleri üzerinde etkili çözümler sunar.

---

## **Neden Kullanılır?**
- **Excel İşlemlerini Otomatikleştirme**: Rapor oluşturma, veri aktarma gibi işlemleri manuel müdahale olmadan programatik olarak gerçekleştirir.
- **Asenkron Yapı**: Uzun sürebilecek Excel işlemlerini asenkron hale getirerek performansı artırır.
- **Gelişmiş Özellikler**: Hücre düzenleme, toplu veri ekleme, resim ekleme, PDF'e dönüştürme ve biçimlendirme işlemlerini destekler.
- **Kolay Kullanım**: API, kullanıcı dostu ve kolay anlaşılır yapısıyla hızlı entegrasyon sağlar.

---

## **Avantajları**
1. **Performanslı**: Büyük veri setlerini hızlı bir şekilde ekleme/düzenleme.
2. **Zengin Özellik Seti**: Sayfa yönetimi, hücre kopyalama, toplu veri ekleme ve PDF dönüştürme desteği.
3. **Asenkron Çalışma**: Tüm işlemler arka planda çalıştırılarak ana uygulamanın performansı korunur.
4. **Esneklik**: Excel dışa aktarma, PDF dönüştürme ve CSV kayıt işlemleri gibi farklı formatları destekler.
5. **Kolay Entegrasyon**: .NET projelerine kolayca eklenir ve hızlı kullanılır.

---

## **Projeye Nasıl Eklenir?**

### **1. NuGet Paket Kurulumu**
Core.Application.Excel kullanmak için aşağıdaki NuGet paketleri projeye eklenmelidir:
```bash
Install-Package ClosedXML
Install-Package System.Threading.Tasks.Extensions
```

### **2. Program.cs Ayarları**
Eğer Dependency Injection (DI) kullanılacaksa, **Program.cs** dosyasında servis eklenir:

```csharp
using Core.Application.Excel.Builders;
using Core.Application.Excel.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Core.Application.Excel servis kaydı
builder.Services.AddSingleton<IExcelBuilderAsync, ExcelBuilderAsync>();

var app = builder.Build();
app.Run();
```

---

## **Detaylı Kullanım Örnekleri**

### **1. Yeni Sayfa Ekleme ve Hücreye Değer Yazma**
```csharp
var excelBuilder = new ExcelBuilderAsync();
await excelBuilder.AddWorksheetAsync("ReportSheet");
await excelBuilder.SetCellValueAsync(1, 1, "Merhaba Dünya!");
await excelBuilder.SaveAsAsync("output.xlsx");
```

### **2. Toplu Veri Ekleme**
Bir listeyi Excel'e toplu halde ekleyebilirsiniz:
```csharp
var data = new List<Person>
{
    new Person { Id = 1, Name = "Ali", Age = 25 },
    new Person { Id = 2, Name = "Ayşe", Age = 30 }
};

await excelBuilder.AddWorksheetAsync("People");
await excelBuilder.SetDataListAsync("A1", data);
await excelBuilder.SaveAsAsync("people.xlsx");

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
}
```

### **3. Satır ve Hücre Kopyalama**
```csharp
await excelBuilder.CopyCellAsync(1, 1, 2, 2, includeFormatting: true);
await excelBuilder.CopyRowAsync(1, 2, includeFormatting: true);
await excelBuilder.SaveAsAsync("copy.xlsx");
```

### **4. Sayfa Ayarları - Orientation, Margin, Page Size**
```csharp
await excelBuilder.SetPageOrientationAsync(true); // Yatay
await excelBuilder.SetPageMarginsAsync(1.0, 1.0, 1.0, 1.0);
await excelBuilder.SetPageSizeAsync("A4");
await excelBuilder.SaveAsAsync("page-settings.xlsx");
```

### **5. Resim Ekleme**
```csharp
await excelBuilder.AddImageAsync("image.png", 1, 1, 5, 5);
await excelBuilder.SaveAsAsync("image.xlsx");
```

### **6. ExportToExcel ve ImportToEntity**
- **Excel'e Veri Aktarma**:
```csharp
var products = new List<Product>
{
    new Product { Id = 1, Name = "Laptop", Price = 1500.0 },
    new Product { Id = 2, Name = "Mouse", Price = 50.0 }
};

await excelBuilder.ExportToExcelAsync("products.xlsx", products);

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double Price { get; set; }
}
```

- **Excel'den Veri Okuma**:
```csharp
var importedData = await excelBuilder.ImportToEntityAsync<Product>("products.xlsx");
foreach (var item in importedData)
{
    Console.WriteLine($"{item.Id}, {item.Name}, {item.Price}");
}
```

### **7. Excel'i PDF Olarak Kaydetme**
```csharp
await excelBuilder.ExportToExcelAsync("report.xlsx", data);
await excelBuilder.SaveAsAsync("report.xlsx", exportToPdf: true);
```

---

## **Sonuç**
**Core.Application.Excel** katmanı, Excel dosyaları ile çalışmak isteyen geliştiriciler için güçlü ve esnek bir araç sunar:
- **Kolay Kullanım**: Sezgisel API yapısı.
- **Asenkron Performans**: Büyük veri setlerinde hızlı işlem.
- **Geniş Özellik Seti**: Hücre düzenleme, toplu veri ekleme, resim ekleme ve PDF dönüştürme.
