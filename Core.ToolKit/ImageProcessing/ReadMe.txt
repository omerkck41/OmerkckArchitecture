# Core.Toolkit Image Processing Module

**Image Processing Module**, resimlerle ilgili bir dizi işlem gerçekleştirmek için güçlü yardımcı metotlar sağlar. Yeniden boyutlandırma, kırpma, format dönüştürme ve sıkıştırma gibi işlemleri yüksek kalite ve detaylı ayarlarla yapmanızı sağlar.

---

## **1. Yeniden Boyutlandırma (ResizeAsync)**

### **Açıklama**
Bir resmi belirtilen genişlik ve yüksekliğe yeniden boyutlandırır. Oran koruma seçeneği ile resimlerin bozulmasını önler.

### **Kullanım**
```csharp
using Core.Toolkit.ImageProcessing;

string inputPath = "path/to/original.jpg";
string outputPath = "path/to/resized.jpg";
int width = 800;
int height = 600;
bool maintainAspectRatio = true;

await ImageHelper.ResizeAsync(inputPath, outputPath, width, height, maintainAspectRatio);
```

---

## **2. Kırpma (CropAsync)**

### **Açıklama**
Bir resmi belirli bir dikdörtgen alanına göre kırpar.

### **Kullanım**
```csharp
using Core.Toolkit.ImageProcessing;
using System.Drawing;

string inputPath = "path/to/original.jpg";
string outputPath = "path/to/cropped.jpg";
Rectangle cropArea = new Rectangle(50, 50, 200, 200);

await ImageHelper.CropAsync(inputPath, outputPath, cropArea);
```

---

## **3. Format Dönüştürme (ConvertFormatAsync)**

### **Açıklama**
Bir resmi farklı bir formata (örneğin PNG, JPEG) dönüştürür.

### **Kullanım**
```csharp
using Core.Toolkit.ImageProcessing;
using System.Drawing.Imaging;

string inputPath = "path/to/original.jpg";
string outputPath = "path/to/converted.png";

await ImageHelper.ConvertFormatAsync(inputPath, outputPath, ImageFormat.Png);
```

---

## **4. Sıkıştırma (CompressAsync)**

### **Açıklama**
Bir resmin dosya boyutunu kalite ayarı yaparak küçültür.

### **Kullanım**
```csharp
using Core.Toolkit.ImageProcessing;

string inputPath = "path/to/original.jpg";
string outputPath = "path/to/compressed.jpg";
long quality = 75; // Kalite 1-100 arasında bir değer

await ImageHelper.CompressAsync(inputPath, outputPath, quality);
```

---

## **Özellikler ve Avantajlar**

- **Yüksek Kalite İşleme**: Tüm grafik işlemleri için yüksek kaliteli interpolasyon ve yumuşatma ayarları uygulanır.
- **Oran Koruma**: Resimleri yeniden boyutlandırırken otomatik olarak oranları koruyabilirsiniz.
- **Esneklik**: Genişletilebilir yapı sayesinde farklı format ve boyut ayarları yapılabilir.
- **Performans**: Async/await ile optimize edilmiş işlem süreleri.

---

## **Pratik Örnekler**

### **Örnek 1: Profil Resmi İşleme**
```csharp
string originalImage = "path/to/profile.jpg";
string resizedImage = "path/to/resized_profile.jpg";

await ImageHelper.ResizeAsync(originalImage, resizedImage, 256, 256, true);
```

### **Örnek 2: Tüm Resimleri Sıkıştırma**
```csharp
string[] imagePaths = Directory.GetFiles("path/to/images", "*.jpg");

foreach (var imagePath in imagePaths)
{
    string compressedPath = Path.ChangeExtension(imagePath, "compressed.jpg");
    await ImageHelper.CompressAsync(imagePath, compressedPath, 50);
}
```

---
