# Core.Toolkit Data Processing Module

**Data Processing Module**, JSON, CSV ve DataTable gibi veri işleme ihtiyaçlarını karşılayan kapsamlı metotlar sunar. Bu metotlar, büyük projelerde güvenilir ve esnek veri işleme yetenekleri sağlar.

---

## **1. JSON İşlemleri (JsonHelper)**

### **Açıklama**
JSON dosyalarını okuma, yazma ve doğrulama işlemlerini destekler.

### **Kullanım**

#### **JSON Dosyasını Okuma**
```csharp
using Core.Toolkit.DataProcessing;

string filePath = "path/to/data.json";
var myObject = await JsonHelper.ReadJsonAsync<MyClass>(filePath);
```

#### **JSON Dosyasına Yazma**
```csharp
using Core.Toolkit.DataProcessing;

string filePath = "path/to/output.json";
var myObject = new MyClass { Property1 = "Value", Property2 = 123 };

await JsonHelper.WriteJsonAsync(myObject, filePath, indented: true);
```

---

## **2. CSV İşlemleri (CsvHelper)**

### **Açıklama**
CSV dosyalarını okuma ve yazma işlemlerini güvenilir ve esnek bir şekilde gerçekleştirir.

### **Kullanım**

#### **CSV Dosyasını Okuma**
```csharp
using Core.Toolkit.DataProcessing;

string filePath = "path/to/data.csv";
var rows = await CsvHelper.ReadCsvAsync(filePath);

foreach (var row in rows)
{
    Console.WriteLine(row["ColumnName"]);
}
```

#### **CSV Dosyasına Yazma**
```csharp
using Core.Toolkit.DataProcessing;

string filePath = "path/to/output.csv";
var data = new List<MyClass>
{
    new MyClass { Property1 = "Value1", Property2 = 123 },
    new MyClass { Property1 = "Value2", Property2 = 456 }
};

await CsvHelper.WriteCsvAsync(data, filePath);
```

---

# Core.Toolkit DataMasker Module

## **1. Masking Sensitive Data**

### **Açıklama**
Hassas verilerin belirli bölümlerini maskeleyerek yalnızca başlangıç ve bitiş karakterlerinin görünmesini sağlar.

### **Kullanım**

#### **Genel Veri Maskeleme**
```csharp
using Core.Toolkit.Utilities;

string maskedData = DataMasker.MaskSensitiveData("SensitiveInformation", 3, 3);
Console.WriteLine(maskedData); // Output: Sen**********ion
```

#### **E-posta Maskeleme**
```csharp
using Core.Toolkit.Utilities;

string maskedEmail = DataMasker.MaskEmail("user@example.com");
Console.WriteLine(maskedEmail); // Output: us****@example.com
```

#### **Telefon Numarası Maskeleme**
```csharp
using Core.Toolkit.Utilities;

string maskedPhone = DataMasker.MaskPhoneNumber("1234567890");
Console.WriteLine(maskedPhone); // Output: ******7890
```

---

## **2. Encrypting and Decrypting Data**

### **Açıklama**
AES algoritması ile veri şifreleme ve çözme işlemlerini gerçekleştirir. Şifrelenmiş veri Base64 formatında döner.

### **Kullanım**

#### **Veri Şifreleme**
```csharp
using Core.Toolkit.Utilities;

string key = "mysecretkey12345";
string encryptedText = DataMasker.Encrypt("SensitiveData", key);
Console.WriteLine(encryptedText); // Output: Base64 şifrelenmiş metin
```

#### **Veri Çözme**
```csharp
using Core.Toolkit.Utilities;

string decryptedText = DataMasker.Decrypt(encryptedText, key);
Console.WriteLine(decryptedText); // Output: SensitiveData
```

---

## **Özellikler ve Avantajlar**

- **Veri Maskeleme**:
  - E-posta, telefon numarası ve genel hassas veriler için özelleştirilebilir maskeleme desteği.

- **AES Şifreleme ve Çözme**:
  - Yüksek güvenlikli şifreleme algoritması kullanımı.
  - Şifreleme anahtarını özelleştirebilme.

- **Performans**:
  - Büyük veri setlerinde hızlı ve güvenilir işlem yapma.

- **Esneklik**:
  - Parametrelerle özelleştirilebilir maskeleme ve şifreleme.

---

## **Pratik Örnekler**

### **Örnek 1: Bir Kullanıcının E-posta Adresini Maskeleme**
```csharp
string email = "user@example.com";
string maskedEmail = DataMasker.MaskEmail(email);
Console.WriteLine(maskedEmail); // Output: us****@example.com
```

### **Örnek 2: Bir Kullanıcı Şifresini Şifreleme ve Çözme**
```csharp
string password = "UserPassword123";
string key = "strongencryptionkey";

string encryptedPassword = DataMasker.Encrypt(password, key);
Console.WriteLine(encryptedPassword);

string decryptedPassword = DataMasker.Decrypt(encryptedPassword, key);
Console.WriteLine(decryptedPassword); // Output: UserPassword123
```



## **3. DataTable İşlemleri (DataTableHelper)**

### **Açıklama**
DataTable ile liste dönüşümleri arasında köprü kurar. Esnek ve hızlı veri işleme sağlar.

### **Kullanım**

#### **DataTable'dan Listeye Dönüşüm**
```csharp
using Core.Toolkit.DataProcessing;
using System.Data;

DataTable dataTable = GetDataTableFromDatabase();
var list = DataTableHelper.ToList<MyClass>(dataTable);
```

#### **Listeden DataTable'a Dönüşüm**
```csharp
using Core.Toolkit.DataProcessing;

var data = new List<MyClass>
{
    new MyClass { Property1 = "Value1", Property2 = 123 },
    new MyClass { Property1 = "Value2", Property2 = 456 }
};

DataTable dataTable = DataTableHelper.ToDataTable(data);
```

---

## **Özellikler ve Avantajlar**

- **Asenkron İşlem**: JSON ve CSV işlemleri async/await desteği ile optimize edilmiştir.
- **Modüler Tasarım**: Metotlar bağımsız ve tekrar kullanılabilir şekilde tasarlanmıştır.
- **Hata Yönetimi**: Kapsamlı hata kontrolleri ve ayrıntılı hata mesajları sağlar.
- **Büyük Projeler için Uygunluk**: Büyük veri setleri ile çalışmak için esnek ve performanslıdır.

---

## **Pratik Örnekler**

### **Örnek 1: JSON Dosyasını Okuma ve Yazma**
```csharp
string inputPath = "path/to/input.json";
string outputPath = "path/to/output.json";

// JSON okuma
var myObject = await JsonHelper.ReadJsonAsync<MyClass>(inputPath);

// JSON yazma
await JsonHelper.WriteJsonAsync(myObject, outputPath);
```

### **Örnek 2: CSV Dosyasından Dinamik Veri Okuma**
```csharp
string csvPath = "path/to/data.csv";
var rows = await CsvHelper.ReadCsvAsync(csvPath);

foreach (var row in rows)
{
    Console.WriteLine(row["Column1"]);
}
```

### **Örnek 3: DataTable ile Çalışma**
```csharp
DataTable dataTable = GetDataTableFromDatabase();

// DataTable'dan listeye dönüşüm
var list = DataTableHelper.ToList<MyClass>(dataTable);

// Listeden DataTable'a dönüşüm
var newDataTable = DataTableHelper.ToDataTable(list);
```

---
