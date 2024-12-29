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
