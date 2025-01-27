# Core.ToolKit.FileManagement

**Core.ToolKit.FileManagement**, dosya yönetimi ile ilgili işlemleri kolaylaştırmak için geliştirilmiş bir .NET kütüphanesidir. Bu kütüphane, dosya okuma/yazma, byte array dönüşümleri, Base64 dönüşümleri, dosya meta verileri okuma ve path işlemleri gibi yaygın dosya yönetimi işlemlerini basit ve etkili bir şekilde gerçekleştirmek için kullanılır.

## **Nedir?**

Core.ToolKit.FileManagement, .NET projelerinde dosya işlemlerini kolaylaştırmak için geliştirilmiş bir araç setidir. Bu kütüphane, dosya okuma/yazma, byte array dönüşümleri, Base64 dönüşümleri, dosya meta verileri okuma ve path işlemleri gibi yaygın işlemleri tek bir çatı altında toplar. Bu sayede, geliştiriciler dosya yönetimi işlemlerini daha hızlı ve hatasız bir şekilde gerçekleştirebilir.

## **Neden Kullanılır?**

- **Kolay Kullanım:** Dosya işlemleri için tekrar eden kod yazmak yerine, bu kütüphane ile hazır metotları kullanabilirsiniz.
- **Performans:** Büyük dosyalar için `Stream` tabanlı işlemlerle performans artışı sağlar.
- **Tutarlılık:** Tüm dosya işlemleri için tutarlı bir API sunar.
- **Genişletilebilirlik:** İhtiyaçlarınıza göre yeni metotlar ekleyerek kütüphaneyi genişletebilirsiniz.

## **Avantajları**

- **Async Destek:** Tüm metotlar async olarak yazılmıştır, bu sayede büyük dosyalarla çalışırken uygulamanızın performansı artar.
- **Hata Yönetimi:** Dosya bulunamaması, geçersiz path gibi durumlar için özel hata mesajları ve exception'lar sunar.
- **Genişletilebilir:** Yeni metotlar ekleyerek kütüphaneyi projenizin ihtiyaçlarına göre özelleştirebilirsiniz.
- **Clean Code:** Metotlar clean code prensiplerine uygun olarak yazılmıştır.

---

## **Kurulum ve Projeye Ekleme**

### 1. **Projeye Ekleme**

Kütüphaneyi projenize eklemek için aşağıdaki adımları izleyin:

1. **NuGet Paketi Olarak Yayınlama:**
   - Eğer bu kütüphaneyi bir NuGet paketi olarak yayınlamak istiyorsanız, `.nuspec` dosyası oluşturup NuGet'e yükleyebilirsiniz.
   - NuGet paketi olarak yayınlandıktan sonra, projenize NuGet üzerinden ekleyebilirsiniz.

2. **DLL Olarak Ekleme:**
   - Kütüphaneyi bir DLL olarak derleyip, projenizin `References` kısmına ekleyebilirsiniz.

3. **Projeye Doğrudan Ekleme:**
   - Eğer kütüphaneyi doğrudan projenize eklemek istiyorsanız, `Core.ToolKit.FileManagement` klasörünü projenize kopyalayıp, gerekli referansları ekleyebilirsiniz.

### 2. **Program.cs ve AppSettings.json Ayarları**

Kütüphaneyi kullanmak için herhangi bir özel ayar yapmanıza gerek yoktur. Ancak, dosya yolları veya dosya boyutu limitleri gibi ayarları `appsettings.json` dosyasında tutabilirsiniz.

#### **appsettings.json Örneği:**

```json
{
  "FileSettings": {
    "MaxFileSizeInBytes": 104857600, // 100 MB
    "DefaultOutputPath": "C:\\OutputFiles"
  }
}
```

#### **Program.cs Örneği:**

```csharp
using Core.ToolKit.FileManagement;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Config dosyasını yükle
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // Ayarları oku
        long maxFileSize = config.GetValue<long>("FileSettings:MaxFileSizeInBytes");
        string outputPath = config.GetValue<string>("FileSettings:DefaultOutputPath");

        // ByteArrayConverter kullanımı
        string filePath = "C:\\ExampleFiles\\example.txt";
        byte[] fileBytes = await ByteArrayConverter.ToByteArrayAsync(filePath, maxFileSize);
        await ByteArrayConverter.FromByteArrayAsync(fileBytes, Path.Combine(outputPath, "output.txt"));

        // FileConverter kullanımı
        string base64String = await FileConverter.ToBase64Async(filePath);
        await FileConverter.FromBase64Async(base64String, Path.Combine(outputPath, "output_base64.txt"));

        // FileMetadataReader kullanımı
        long fileSize = FileMetadataReader.GetFileSize(filePath);
        DateTime creationDate = FileMetadataReader.GetCreationDate(filePath);
        DateTime lastModifiedDate = FileMetadataReader.GetLastModifiedDate(filePath);

        // PathHelper kullanımı
        bool isValidPath = PathHelper.IsValidPath(filePath);
        bool hasTxtExtension = PathHelper.HasExtension(filePath, ".txt");
    }
}
```

---

## **Detaylı Kullanım Örnekleri**

### 1. **ByteArrayConverter Kullanımı**

#### **Dosyayı Byte Array'e Dönüştürme:**

```csharp
string filePath = "C:\\ExampleFiles\\example.txt";
byte[] fileBytes = await ByteArrayConverter.ToByteArrayAsync(filePath);
```

#### **Byte Array'i Dosyaya Yazma:**

```csharp
string outputPath = "C:\\OutputFiles\\output.txt";
await ByteArrayConverter.FromByteArrayAsync(fileBytes, outputPath);
```

#### **Stream ile Byte Array'e Dönüştürme:**

```csharp
using (var stream = new FileStream(filePath, FileMode.Open))
{
    byte[] fileBytes = await ByteArrayConverter.StreamToByteArrayAsync(stream);
}
```

---

### 2. **FileConverter Kullanımı**

#### **Dosyayı Base64'e Dönüştürme:**

```csharp
string base64String = await FileConverter.ToBase64Async(filePath);
```

#### **Base64'ten Dosya Oluşturma:**

```csharp
await FileConverter.FromBase64Async(base64String, outputPath);
```

#### **Stream ile Base64'e Dönüştürme:**

```csharp
using (var stream = new FileStream(filePath, FileMode.Open))
{
    string base64String = await FileConverter.StreamToBase64Async(stream);
}
```

---

### 3. **FileMetadataReader Kullanımı**

#### **Dosya Boyutunu Okuma:**

```csharp
long fileSize = FileMetadataReader.GetFileSize(filePath);
```

#### **Dosya Oluşturma Tarihini Okuma:**

```csharp
DateTime creationDate = FileMetadataReader.GetCreationDate(filePath);
```

#### **Dosya Özniteliklerini Okuma:**

```csharp
FileAttributes attributes = FileMetadataReader.GetFileAttributes(filePath);
```

---

### 4. **PathHelper Kullanımı**

#### **Path Birleştirme:**

```csharp
string combinedPath = PathHelper.Combine("C:\\ExampleFiles", "subfolder", "example.txt");
```

#### **Path Validasyonu:**

```csharp
bool isValid = PathHelper.IsValidPath("C:\\ExampleFiles\\example.txt");
```

#### **Dosya Uzantısı Kontrolü:**

```csharp
bool hasTxtExtension = PathHelper.HasExtension("C:\\ExampleFiles\\example.txt", ".txt");
```

---