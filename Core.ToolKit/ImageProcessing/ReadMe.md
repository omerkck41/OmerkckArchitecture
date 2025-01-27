# ImageHelper Kütüphanesi

## Nedir?
`ImageHelper`, görüntü işleme işlemlerini kolaylaştırmak için geliştirilmiş bir C# kütüphanesidir. Bu kütüphane, **SixLabors.ImageSharp** kütüphanesini kullanarak görüntüleri yeniden boyutlandırma, kırpma, format dönüştürme, sıkıştırma, döndürme ve filtreleme gibi işlemleri asenkron olarak gerçekleştirir. Büyük projelerde rahatça kullanılabilir ve temiz kod prensiplerine uygun olarak tasarlanmıştır.

---

## Neden Kullanılır?
- **Kolay Kullanım**: Görüntü işleme işlemlerini birkaç satır kodla gerçekleştirebilirsiniz.
- **Esneklik**: Yeniden boyutlandırma, kırpma, format dönüştürme, sıkıştırma ve daha birçok işlemi destekler.
- **Performans**: Asenkron yapısı sayesinde büyük görüntüler üzerinde yüksek performans sağlar.
- **Modüler Yapı**: Yeni özellikler eklemek veya mevcut özellikleri genişletmek kolaydır.

---

## Avantajları
- **Platform Bağımsız**: .NET Core ve .NET 5/6/7 ile uyumludur.
- **Geniş Format Desteği**: JPEG, PNG, WebP gibi popüler görüntü formatlarını destekler.
- **Temiz Kod**: Best practice'lere uygun olarak geliştirilmiştir.
- **Hata Yönetimi**: Tüm işlemlerde hata yönetimi ve loglama mekanizmaları bulunur.
- **Genişletilebilirlik**: Yeni filtreler veya işlemler kolayca eklenebilir.

---

## Projeye Ekleme ve Ayarlar

### 1. **Projeye Ekleme**
Öncelikle, `SixLabors.ImageSharp` kütüphanesini projenize ekleyin. Bu kütüphane, `ImageHelper` tarafından kullanılmaktadır.

```bash
dotnet add package SixLabors.ImageSharp
```

### 2. **ImageHelper Sınıfını Ekleyin**
`ImageHelper.cs` dosyasını projenize ekleyin veya bir kütüphane olarak derleyin.

### 3. **Program.cs ve AppSettings.json Ayarları**
`ImageHelper` sınıfını kullanmak için herhangi bir özel ayar gerekmemektedir. Ancak, görüntü işleme işlemleri için gerekli parametreleri `appsettings.json` dosyasında saklayabilirsiniz.

#### Örnek `appsettings.json`
```json
{
  "ImageProcessingSettings": {
    "DefaultOutputPath": "wwwroot/images/processed",
    "DefaultQuality": 85,
    "DefaultWidth": 800,
    "DefaultHeight": 600
  }
}
```

#### Örnek `Program.cs` veya `Startup.cs`
```csharp
using Core.ToolKit.ImageProcessing;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var app = builder.Build();

app.MapGet("/process-image", async (context) =>
{
    var inputPath = "wwwroot/images/input.jpg";
    var outputPath = Path.Combine(configuration["ImageProcessingSettings:DefaultOutputPath"], "output.jpg");
    var width = int.Parse(configuration["ImageProcessingSettings:DefaultWidth"]);
    var height = int.Parse(configuration["ImageProcessingSettings:DefaultHeight"]);

    await ImageHelper.ResizeAsync(inputPath, outputPath, width, height);

    await context.Response.WriteAsync("Image processed successfully!");
});

app.Run();
```

---

## Detaylı Kullanım Örnekleri

### 1. **Görüntüyü Yeniden Boyutlandırma**
```csharp
await ImageHelper.ResizeAsync("input.jpg", "output.jpg", 800, 600, maintainAspectRatio: true);
```

### 2. **Görüntüyü Kırpma**
```csharp
var cropArea = new Rectangle(100, 100, 400, 300); // x, y, width, height
await ImageHelper.CropAsync("input.jpg", "output.jpg", cropArea);
```

### 3. **Görüntü Formatını Dönüştürme**
```csharp
var encoder = new SixLabors.ImageSharp.Formats.Png.PngEncoder();
await ImageHelper.ConvertFormatAsync("input.jpg", "output.png", encoder);
```

### 4. **Görüntüyü Sıkıştırma**
```csharp
await ImageHelper.CompressAsync("input.jpg", "output.jpg", quality: 75);
```

### 5. **Görüntüyü Döndürme**
```csharp
await ImageHelper.RotateAsync("input.jpg", "output.jpg", degrees: 90);
```

### 6. **Görüntüye Filtre Uygulama**
```csharp
await ImageHelper.ApplyFilterAsync("input.jpg", "output.jpg", ctx => ctx.Grayscale());
```

### 7. **Görüntü Boyutlarını Alma**
```csharp
var dimensions = await ImageHelper.GetImageDimensionsAsync("input.jpg");
Console.WriteLine($"Width: {dimensions.Width}, Height: {dimensions.Height}");
```

---