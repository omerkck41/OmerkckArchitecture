# OTP & MFA Authentication Service

## 📌 Nedir?
Bu proje, güvenli kimlik doğrulama işlemlerini sağlamak için **OTP (One-Time Password)** ve **MFA (Multi-Factor Authentication)** mekanizmalarını kullanır. **TOTP (Time-Based One-Time Password)** algoritması temel alınarak **Google Authenticator, Microsoft Authenticator, Email ve SMS doğrulama** gibi yöntemleri destekler.

## 🎯 Neden Kullanılır?
- **İki Faktörlü Kimlik Doğrulama (2FA)** gerektiren uygulamalarda,
- **Google Authenticator gibi TOTP tabanlı doğrulama uygulamalarıyla** entegrasyon sağlamak için,
- **E-posta ve SMS doğrulama** işlemlerinde,
- **OTP ile kullanıcı girişini güvenli hale getirmek** için kullanılır.

## 🚀 Avantajları
- **Modüler ve genişletilebilir yapı** sayesinde farklı kimlik doğrulama yöntemleri eklenebilir.
- **TOTP (Google Authenticator) desteği** ile güvenliği artırır.
- **Email ve SMS OTP desteği** ile kullanıcıya çoklu doğrulama yöntemi sunar.
- **Dependency Injection (DI) ile kolay yönetilebilir**.

---

## 🔧 Projeye Nasıl Eklenir?

### **1️⃣ NuGet Paketlerini Yükleyin**
Projeye OTP desteği için aşağıdaki NuGet paketlerini ekleyin:
```shell
 dotnet add package Otp.NET
```

### **2️⃣ Dependency Injection Ayarları (Program.cs)**
Aşağıdaki kod ile servisleri `Program.cs` dosyanızda kaydedin:
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IOtpService, TotpService>();
builder.Services.AddScoped<IMfaService, MfaService>();

var app = builder.Build();
app.Run();
```

---

## 📂 **appsettings.json Konfigürasyonu**
Eğer OTP doğrulamalarında süre veya uzunluk gibi özel ayarlamalar yapacaksanız, `appsettings.json` dosyanıza aşağıdaki gibi bir yapı ekleyebilirsiniz:
```json
{
  "OtpSettings": {
    "OtpLength": 6,
    "OtpExpirySeconds": 30
  }
}
```

---

## 📌 **Detaylı Kullanım Örnekleri**

### 🔹 **1️⃣ OTP Kodu Üretme ve Doğrulama**
```csharp
var otpService = serviceProvider.GetRequiredService<IOtpService>();
string secretKey = await otpService.GenerateSecretKey();
string otp = await otpService.GenerateOtpCodeAsync(secretKey);

Console.WriteLine($"OTP Kodu: {otp}");

bool isValid = await otpService.ValidateOtpCodeAsync(secretKey, otp);
Console.WriteLine(isValid ? "✅ OTP Geçerli!" : "❌ Hatalı OTP!");
```

### 🔹 **2️⃣ Google Authenticator ile Entegrasyon**
```csharp
var otpService = serviceProvider.GetRequiredService<IOtpService>();
string secretKey = await otpService.GenerateSecretKey();
string otpAuthUrl = await otpService.GenerateOtpAuthUrlAsync("user@example.com", "MyApp", secretKey);

Console.WriteLine("Google Authenticator için aşağıdaki QR kodu taratın:");
Console.WriteLine(otpAuthUrl);
```

### 🔹 **3️⃣ Email & SMS OTP Kullanımı**
```csharp
var mfaService = serviceProvider.GetRequiredService<IMfaService>();

string emailOtp = await mfaService.GenerateEmailCodeAsync();
Console.WriteLine($"Email OTP: {emailOtp}");

bool isEmailValid = await mfaService.ValidateEmailCodeAsync(emailOtp, emailOtp);
Console.WriteLine(isEmailValid ? "✅ Email OTP Geçerli!" : "❌ Hatalı Email OTP!");

string smsOtp = await mfaService.GenerateSmsCodeAsync();
Console.WriteLine($"SMS OTP: {smsOtp}");

bool isSmsValid = await mfaService.ValidateSmsCodeAsync(smsOtp, smsOtp);
Console.WriteLine(isSmsValid ? "✅ SMS OTP Geçerli!" : "❌ Hatalı SMS OTP!");
```

---

## 📌 **Sonuç**
Bu yapı, **OTP tabanlı güvenli kimlik doğrulama işlemlerini yönetmek için esnek ve ölçeklenebilir bir çözüm sunar**. Projeye kolayca entegre edilebilir ve **Google Authenticator, Email ve SMS doğrulama işlemleriyle güvenliği artırabilir**.

🔹 **Daha fazla geliştirme veya ekleme yapmak isterseniz, haber verin!** 🚀

