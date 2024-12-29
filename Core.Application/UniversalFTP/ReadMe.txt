# UniversalFTP Kullanım Rehberi

Bu rehber, UniversalFTP projesinin projeye nasıl entegre edileceğini, yapılandırılacağını ve kullanılacağını açıklamaktadır. Ayrıca, **Transaction**, **Retry**, ve **Bağlantı Havuzu** gibi özelliklerin nasıl kullanılacağına dair detaylı açıklamalar içerir.

---

## 1. Projeye Nasıl Entegre Edilir?

1. **UniversalFTP NuGet Paketi Eklenmesi**  
   UniversalFTP, bir **Class Library** olarak geliştirilmiştir. Projenize eklemek için bu kütüphaneyi NuGet paketi olarak yükleyin:
   ```bash
   dotnet add package UniversalFTP
   ```

2. **FtpSettings Ayarlarının Eklenmesi**  
   `appsettings.json` dosyasına FTP bağlantı bilgilerini ekleyin:
   ```json
   {
       "FtpSettings": {
           "Host": "ftp.example.com",
           "Port": 21,
           "Username": "ftpuser",
           "Password": "ftppassword",
           "UseSsl": false,
           "RetryCount": 3,
           "ConnectTimeout": "00:00:10",
           "ReadTimeout": "00:00:30"
       }
   }
   ```

---

## 2. ServiceCollection Kullanılarak Nasıl Yapılandırılır?

`ServiceCollectionExtensions` sınıfını kullanarak FTP servislerini kolayca yapılandırabilirsiniz.

### Kullanım Örneği
`Program.cs` dosyasına aşağıdaki kodu ekleyin:
```csharp
var builder = WebApplication.CreateBuilder(args);

// FtpSettings'i ve servisleri ekle
builder.Services.AddUniversalFtpServices(builder.Configuration.GetSection("FtpSettings"));

// Uygulama oluştur
var app = builder.Build();
app.Run();
```

### Alternatif Yapılandırma
Eğer **FtpSettings**'i manuel olarak ayarlamak isterseniz:
```csharp
var ftpSettings = new FtpSettings
{
    Host = "ftp.example.com",
    Port = 21,
    Username = "ftpuser",
    Password = "ftppassword",
    UseSsl = false,
    RetryCount = 3
};

builder.Services.AddUniversalFtpServices(ftpSettings);
```

---

## 3. FtpService ve FtpDirectoryService Kullanım Örnekleri

### FtpService Kullanımı
**Dosya Yükleme ve İndirme**
```csharp
var ftpService = app.Services.GetRequiredService<IFtpService>();

// Dosya yükleme
var uploadResult = await ftpService.UploadFileAsync("local/path/file.txt", "/remote/path/file.txt");
Console.WriteLine(uploadResult.Message);

// Dosya indirme
var downloadResult = await ftpService.DownloadFileAsync("/remote/path/file.txt", "local/path/file.txt");
Console.WriteLine(downloadResult.Message);
```

### FtpDirectoryService Kullanımı
**Klasör Oluşturma ve Listeleme**
```csharp
var ftpDirectoryService = app.Services.GetRequiredService<IFtpDirectoryService>();

// Klasör oluşturma
await ftpDirectoryService.CreateDirectoryAsync("/remote/path/newFolder");

// Klasör listeleme
var directories = await ftpDirectoryService.ListDirectoriesAsync("/remote/path");
directories.ToList().ForEach(Console.WriteLine);
```

---

## 4. Transaction ve Retry Özelliklerinin Açıklamaları

### Transaction Desteği
FTP işlemlerini bir **Transaction** içinde gerçekleştirmek için `FluentFtpTransactionService` kullanabilirsiniz.

**Kullanım Örneği**
```csharp
var ftpTransactionService = app.Services.GetRequiredService<FluentFtpTransactionService>();

await ftpTransactionService.BeginTransactionAsync(async context =>
{
    await context.FtpService.UploadFileAsync("local/path/file1.txt", "/remote/path/file1.txt");
    await context.FtpDirectoryService.CreateDirectoryAsync("/remote/path/newFolder");
});
```

### Retry Mekanizması
Hata durumunda işlemi tekrar denemek için **Retry** mekanizması bulunmaktadır.

**Kullanım Örneği**
```csharp
var ftpRetryService = app.Services.GetRequiredService<FluentFtpRetryService>();

await ftpRetryService.RetryAsync(async () =>
{
    await ftpService.UploadFileAsync("local/path/file.txt", "/remote/path/file.txt");
}, retryCount: 3);
```

---

## 5. Gelişmiş Özellikler

1. **Bağlantı Havuzu:**  
   Tüm FTP işlemleri için `FtpConnectionPool` kullanılır, böylece her işlemde yeni bir bağlantı açılmasına gerek kalmaz.

2. **Büyük Dosyalar İçin Parçalama Desteği:**  
   Büyük dosyalar küçük parçalara ayrılarak FTP'ye yüklenebilir:
   ```csharp
   var chunkedFtpService = app.Services.GetRequiredService<IChunkedFtpService>();
   await chunkedFtpService.UploadLargeFileAsync("local/largefile.zip", "/remote/largefile.zip");
   ```

---

## 6. Projenin Avantajları

1. **Esneklik:**  
   - Hem bağımsız servisler hem de MediatR Behaviors desteklenir.

2. **Performans:**  
   - Bağlantı havuzu sayesinde bağlantı yönetimi optimize edilmiştir.

3. **Kullanım Kolaylığı:**  
   - Modern ve temiz API tasarımı ile hızlı entegrasyon imkanı.

4. **Genişletilebilirlik:**  
   - Özelleştirilebilir `Retry` ve `Transaction` desteği mevcuttur.