# Core.Integration - Kullanım ve Entegrasyon Rehberi

## **Nedir?**
Core.Integration modülü, harici API servisleriyle iletişim kurmak için güçlü bir entegrasyon altyapısı sunar. **HTTP istemcisi**, **standart API istek ve cevap modelleri** ve **üçüncü taraf servis entegrasyonu** gibi bileşenleri içerir.

---

## **Neden Kullanılır?**
Modern uygulamalarda sıkça kullanılan üçüncü taraf servisler veya harici API'lerle entegrasyonu kolaylaştırmak için Core.Integration modülü şu nedenlerle kullanılır:
- **Standartlaştırma**: Tüm API çağrıları için ortak bir yapı sağlar.
- **Kolay Konfigürasyon**: appsettings.json üzerinden yapılandırılabilir.
- **HTTP İstemcisi Yönetimi**: Verimli ve tekrar kullanılabilir HTTP istemcisi sağlar.

---

## **Avantajları**
1. **Modüler ve Genişletilebilir Yapı**:
   - Yeni servis entegrasyonlarını kolayca ekleyebilirsiniz.

2. **Esnek ve Güvenli**:
   - API anahtarları ve endpoint'ler gibi hassas bilgileri merkezi bir yerde yönetir.

3. **Standart Modelleme**:
   - İstek ve cevap modelleri ile tutarlı bir veri yapısı sağlar.

4. **Verimlilik**:
   - HttpClientFactory kullanarak bağlantı havuzlamasını destekler.

---

## **Projeye Entegrasyon**

### **1. Gerekli NuGet Paketlerinin Kurulumu**
Core.Integration modülünü kullanabilmek için aşağıdaki NuGet paketlerini ekleyin:

```bash
Install-Package Microsoft.Extensions.Http
Install-Package Microsoft.Extensions.Options.ConfigurationExtensions
```

---

### **2. Program.cs Yapılandırması**
Core.Integration modülünü projenize eklemek için aşağıdaki kodları kullanın:

#### **Servislerin Eklenmesi**
```csharp
using Core.Integration.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Core.Integration servislerini ekle
builder.Services.AddIntegrationServices(builder.Configuration);

var app = builder.Build();

// API çalıştır
app.Run();
```

---

### **3. appsettings.json Yapılandırması**
Core.Integration modülünde kullanılan üçüncü taraf API ayarlarını yapılandırmak için `appsettings.json` dosyasına aşağıdaki bölümü ekleyin:

```json
{
  "ThirdPartySettings": {
    "ApiKey": "YourSecureApiKey",
    "Endpoint": "https://api.thirdparty.com/endpoint"
  }
}
```

- **ApiKey**: Harici API için kimlik doğrulama anahtarı.
- **Endpoint**: API'nin temel adresi.

---

## **Detaylı Kullanım Örnekleri**

### **1. API İsteği Gönderme**
Harici bir API'ye istek göndermek için:

```csharp
var apiClient = app.Services.GetRequiredService<IApiClient>();
var request = new ApiRequest
{
    Url = "https://api.thirdparty.com/data",
    Method = HttpMethod.Get,
    Headers = new Dictionary<string, string>
    {
        { "Authorization", "Bearer YourSecureApiKey" }
    }
};

var response = await apiClient.SendRequestAsync<dynamic>(request);

if (response.IsSuccess)
{
    Console.WriteLine("Data: " + response.Data);
}
else
{
    Console.WriteLine("Error: " + response.ErrorMessage);
}
```

---

### **2. Ücüncü Taraf Servis Entegrasyonu**
Özel bir üçüncü taraf entegrasyon servisini kullanmak için:

```csharp
var integrationService = app.Services.GetRequiredService<IThirdPartyIntegrationService>();
var result = await integrationService.PerformIntegrationAsync("Sample Data");

if (result.IsSuccess)
{
    Console.WriteLine("Integration successful: " + result.Data);
}
else
{
    Console.WriteLine("Integration failed: " + result.ErrorMessage);
}
```

---

### **3. Yeni Bir API Entegrasyonu Eklemek**
Yeni bir API entegrasyonu eklemek için **IApiClient**'i kullanabilirsiniz:

1. **Yeni Servis Tanımlayın**:
```csharp
public class NewServiceIntegration : IThirdPartyIntegrationService
{
    private readonly IApiClient _apiClient;

    public NewServiceIntegration(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<ApiResponse<string>> PerformIntegrationAsync(string data)
    {
        var request = new ApiRequest
        {
            Url = "https://api.newservice.com/data",
            Method = HttpMethod.Post,
            Body = new { data }
        };

        return await _apiClient.SendRequestAsync<string>(request);
    }
}
```

2. **Servisleri Ekleyin**:
```csharp
builder.Services.AddScoped<IThirdPartyIntegrationService, NewServiceIntegration>();
```

---

## **Sonuç**
Core.Integration modülü, projelerinizde harici API'lerle iletişim kurmayı standartlaştırır ve kolaylaştırır.
Esnek yapısı sayesinde farklı API'ler arasında hızlıca geçiş yapabilir ve kod tekrarını en aza indirebilirsiniz.
Büyük projelerde harici entegrasyon ihtiyaçlarınızı güvenli ve düzenli bir şekilde yönetmek için idealdir.
