# OmerkckArchitecture

**OmerkckArchitecture**, büyük çaplı projelerde yeniden kullanılabilir, genişletilebilir ve modüler bir mimari sağlayan, **C# .NET 9.0** tabanlı bir yazılım geliştirme çözümüdür. Clean Code ve SOLID prensiplerini temel alan bu proje, modern yazılım ihtiyaçlarını karşılamak üzere tasarlanmıştır.

---

## Proje Yapısı

Projede aşağıdaki modüller bulunmaktadır:

### 1. **Core.API**
Projenin API katmanındaki ana özellikler:
- **Attributes**: Özel API davranışlarını tanımlayan nitelikler.
- **Configurations**: API yapılandırmalarını içerir.
- **Controllers**: RESTful API uç noktaları.
- **Extensions**: API için genişletme metotları.
- **Filters**: Özel filtreleme mekanizmaları.
- **Middlewares**: API özel middleware bileşenleri.

### 2. **Core.Application**
Bu katman iş mantığını yönetir ve aşağıdaki modülleri kapsar:
- **Authorization**: Yetkilendirme mekanizmaları.
- **Caching**: Performansı artıran cache yönetimi.
- **ElasticSearch**: Arama ve loglama için ElasticSearch entegrasyonu.
- **Excel**: Excel dosya işlemleri.
- **Mailing**: SMTP tabanlı e-posta gönderimi.
- **Validation**: Veri doğrulama işlemleri.

### 3. **Core.BackgroundJobs**
Arka plan işlemlerini yönetir:
- **Jobs**: Zamanlanmış işlem yönetimi.
- **Services**: Arka plan iş hizmetleri.
- **Interfaces**: İşlem arayüzleri.

### 4. **Core.Integration**
Dış sistemlerle entegrasyon sağlar:
- **Models**: Veri modelleri.
- **Jobs**: Entegrasyon işlemleri.
- **Services**: Dış servislerle iş akışları.

### 5. **Core.Persistence**
Veri erişim katmanı:
- **Entities**: Veritabanı tabloları için entity tanımlamaları.
- **Repositories**: Generic repository uygulamaları.
- **Paging**: Veri listeleme ve sayfalama mekanizmaları.
- **UnitOfWork**: Transaction yönetimi.

### 6. **Core.Security**
Güvenlik altyapısını sağlar:
- **Encryption**: Şifreleme ve veri güvenliği.
- **JWT**: Token tabanlı kimlik doğrulama.
- **MFA**: İki faktörlü kimlik doğrulama.
- **OAuth**: Harici kimlik doğrulama sistemleri.
- **Validators**: Güvenlik doğrulama mekanizmaları.

### 7. **Core.ToolKit**
Projeyi destekleyen yardımcı araçlar:
- **FileManagement**: Dosya yükleme ve yönetimi.
- **ImageProcessing**: Görüntü işleme.
- **Localization**: Çok dilli destek sistemleri.
- **SessionManagement**: Oturum yönetimi ve güvenlik.

---
