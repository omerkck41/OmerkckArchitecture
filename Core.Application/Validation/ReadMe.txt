# Validation Katmanı Kullanımı ve Faydaları

## **1. Validation Katmanını Projeye Entegre Etme**

Oluşturduğumuz `Core.Application.Validation` katmanını projeye entegre etmek için aşağıdaki adımları takip edebilirsiniz:

### **Adım 1: NuGet Paketlerini Yükleme**
Projede aşağıdaki FluentValidation ve MediatR paketlerinin kurulu olduğundan emin olun:

```bash
Install-Package FluentValidation
Install-Package MediatR.Extensions.Microsoft.DependencyInjection
```

### **Adım 2: Dependency Injection Yapılandırması**
Validation katmanındaki bağımlılıkları projeye eklemek için `Program.cs` veya `Startup.cs` dosyasına şu kodları ekleyin:

```csharp
using Core.Application.Validation.Behaviors;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// FluentValidation assembly yükleme
builder.Services.AddValidatorsFromAssembly(typeof(RequestValidationBehavior<,>).Assembly);

// MediatR pipeline'a validasyon ekleme
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));

var app = builder.Build();
app.Run();
```

### **Adım 3: DTO (Data Transfer Object) ve Validatör Tanımları**
İstekler ve validasyon kurallarınızı şu şekilde tanımlayabilirsiniz:

```csharp
using Core.Application.Validation.Validators;

public class CreateUserRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
}

public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Email).MustBeValidEmail();
        RuleFor(x => x.Age).GreaterThan(18).WithMessage("Age must be greater than 18.");
    }
}
```

### **Adım 4: MediatR Handler Üzerinde Kullanımı**

Validation katmanı, MediatR pipeline'ında otomatik olarak çalıştırılır. Örneğin:

```csharp
using MediatR;

public class CreateUserCommand : IRequest<string>
{
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
}

public class CreateUserHandler : IRequestHandler<CreateUserCommand, string>
{
    public Task<string> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // İş mantığı burada uygulanır
        return Task.FromResult("User created successfully.");
    }
}
```

---

## **2. Validation Katmanının Faydaları**

### **2.1. Tekrarlayan Koddan Kurtulma**
- Tüm validasyonlar merkezi bir katmanda tanımlanır ve bu sayede her istekte tekrarlanan validasyon kodlarını yazmaktan kaçınırsınız.

### **2.2. Test Edilebilirlik**
- Validasyon kuralları `AbstractValidator` üzerinden kolayca test edilebilir. İş mantığından bağımsız olarak doğrulama işlemleri kontrol edilir.

### **2.3. Esneklik ve Modülerlik**
- Proje büyüdükçe validasyon kurallarını genişletmek veya özelleştirmek oldukça kolaydır.

### **2.4. Hata Yönetimi Kolaylığı**
- `ValidationResultFormatter` gibi bir yapı ile validasyon hatalarını anlamlı mesajlar olarak dönebilirsiniz.

---

## **3. Validasyonun Ne İşe Yaradığı ve Avantajları**

### **Validasyon Nedir?**
Validasyon, bir kullanıcının veya sistemin gönderdiği verilerin işlenmeden önce doğruluğunu ve güvenilirliğini kontrol eden bir süreçtir.

### **Başlıca Avantajları:**
1. **Veri Tutarlılığı Sağlama:** Yanlış veya eksik veri ile sistemin çalışmasını önler.
2. **Kullanıcı Deneyimi Geliştirme:** Hatalı girişlerde kullanıcıya anlamlı geri bildirimler sunar.
3. **Güvenlik:** Beklenmeyen veri girişleri (örneğin SQL Injection) için koruma sağlar.
4. **Kodun Temiz ve Okunabilir Olması:** Validasyon kuralları ayrı bir katmanda tanımlandığı için iş mantığı net kalır.

---

## **4. Örnek Kullanım Senaryoları**

### **4.1. Form Doğrulama**
Kullanıcının bir kayıt formu doldurduğunu varsayalım. Validasyon sayesinde:
- Email formatını kontrol edebilirsiniz.
- Şifrenin gerekli uzunlukta ve komplekslikte olduğunu doğrulayabilirsiniz.
- Yaşın belirli bir aralıkta olup olmadığını kontrol edebilirsiniz.

### **4.2. API Doğrulama**
Bir REST API'de istemciden gelen JSON verisinin geçerli olduğundan emin olabilirsiniz. Örneğin, tarih aralıkları veya kimlik formatları doğrulanabilir.

---