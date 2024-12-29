# Core.Toolkit Dosya Yönetimi Modülü

**Dosya Yönetimi Modülü**, Core.Toolkit kütüphanesinde dosya ve dizin işlemlerini kolaylaştıran araçlar sunar. Bu araçlar dosya formatı dönüştürme, byte dizisi işlemleri ve dosya meta bilgilerini alma gibi işlemleri destekler. Aşağıda her yardımcı sınıfın açıklamaları ve kullanım örnekleri bulunmaktadır.

---

## **1. FileConverter**

### **Açıklama**
Dosyaları Base64 string’ine ve Base64 string’inden dosyaya dönüştürme işlemlerini sağlar.

### **Kullanım**
```csharp
using Core.Toolkit.FileManagement;

// Dosyayı Base64 string’ine dönüştürme
string filePath = "dosya/yolu/ornek.txt";
string base64String = await FileConverter.ToBase64Async(filePath);

// Base64 string’ini dosyaya dönüştürme
string outputPath = "dosya/yolu/cikti.txt";
await FileConverter.FromBase64Async(base64String, outputPath);
```

---

## **2. ByteArrayConverter**

### **Açıklama**
Dosyaları byte dizisine ve byte dizisinden dosyaya dönüştürme işlemlerini sağlar.

### **Kullanım**
```csharp
using Core.Toolkit.FileManagement;

// Dosyayı byte dizisine dönüştürme
string filePath = "dosya/yolu/ornek.jpg";
byte[] fileBytes = await ByteArrayConverter.ToByteArrayAsync(filePath);

// Byte dizisini dosyaya yazma
string outputPath = "dosya/yolu/cikti.jpg";
await ByteArrayConverter.FromByteArrayAsync(fileBytes, outputPath);
```

---

## **3. PathHelper**

### **Açıklama**
Dosya ve dizin yollarıyla ilgili işlemleri kolaylaştırır.

### **Kullanım**
```csharp
using Core.Toolkit.FileManagement;

// Yolları birleştirme
string combinedPath = PathHelper.Combine("C:\\Klasor1", "AltKlasor", "dosya.txt");

// Yolun geçerli olup olmadığını doğrulama
bool isValid = PathHelper.IsValidPath(combinedPath);

// Dosya uzantısını kontrol etme
bool hasTxtExtension = PathHelper.HasExtension(combinedPath, ".txt");

// Belirli bir uzantıya sahip tüm dosya yollarını alma
string directoryPath = "C:\\Klasor1";
var filePaths = PathHelper.GetFilePaths(directoryPath, "*.txt", SearchOption.AllDirectories);

// Alt dizin yollarını alma
var directoryPaths = PathHelper.GetDirectoryPaths(directoryPath, SearchOption.TopDirectoryOnly);
```

---

## **4. FileMetadataReader**

### **Açıklama**
Dosya hakkında boyut, oluşturulma tarihi ve son değiştirilme tarihi gibi meta bilgileri alır.

### **Kullanım**
```csharp
using Core.Toolkit.FileManagement;

// Dosya boyutunu alma
string filePath = "dosya/yolu/ornek.txt";
long fileSize = FileMetadataReader.GetFileSize(filePath);

// Dosyanın oluşturulma tarihini alma
DateTime creationDate = FileMetadataReader.GetCreationDate(filePath);

// Dosyanın son değiştirilme tarihini alma
DateTime lastModifiedDate = FileMetadataReader.GetLastModifiedDate(filePath);
```

---

## **5. Pratik Örnekler**

### **Örnek 1: Bir Dosyayı Yedekleme**
```csharp
string sourceFilePath = "dosya/yolu/orijinal.txt";
string backupFilePath = "dosya/yolu/yedek.txt";

// Dosyayı Base64 string’e dönüştürme
string base64Data = await FileConverter.ToBase64Async(sourceFilePath);

// Base64 string’inden dosyayı geri yükleme
await FileConverter.FromBase64Async(base64Data, backupFilePath);
```

### **Örnek 2: Belirli Uzantıya Sahip Dosyaları Listeleme**
```csharp
string directoryPath = "dosya/yolu/klasor";
var imagePaths = PathHelper.GetFilePaths(directoryPath, "*.jpg", SearchOption.AllDirectories);

foreach (var imagePath in imagePaths)
{
    Console.WriteLine(imagePath);
}
```

---

## **Özellikler ve Avantajlar**

- **Asenkron Metotlar**: Tüm dosya işlemleri async/await ile optimize edilmiştir.
- **Modüler Tasarım**: Her yardımcı sınıf bağımsız ve tekrar kullanılabilir şekilde tasarlanmıştır.
- **Hata Yönetimi**: Çalışma zamanı hatalarını önlemek için kapsamlı hata kontrolü.
- **Genişletilebilirlik**: İhtiyaç duyulduğunda ek özellikler kolayca entegre edilebilir.

---
