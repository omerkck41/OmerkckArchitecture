# Core.StatusHandling

ASP.NET Core uygulamaları için merkezi HTTP durum kodu (StatusCode) yönetim kütüphanesi.

## Amaç

Bu kütüphane, ASP.NET Core MVC, Razor Pages, Minimal API veya Blazor projelerinde sık karşılaşılan HTTP durum kodlarını (401, 403, 404, 500 vb.) merkezi bir yerden, kullanıcı dostu bir şekilde yönetmeyi ve isteğe bağlı olarak UI bildirimleri (Toast vb.) tetiklemeyi amaçlar.

## Kurulum

Bu kütüphaneyi projenize ekleyin (örneğin bir submodule olarak).

## Kullanım

1.  **Servisleri Kaydetme (`Program.cs` veya `Startup.cs`)**

    `Program.cs` (.NET 6+ Minimal API şablonu):

    ```csharp
    using Core.StatusHandling.Extensions;
    using Core.StatusHandling.Models; // StatusCodeHandlingOptions için

    var builder = WebApplication.CreateBuilder(args);

    // Diğer servisler...
    builder.Services.AddControllersWithViews(); // veya AddRazorPages(), AddMinimalApis() vb.

    // Status Code Handling servislerini ekle ve yapılandır
    builder.Services.AddStatusCodeHandling(options =>
    {
        // İsteğe bağlı yapılandırma
        options.EnableNotifications = true; // Bildirimleri etkinleştir (varsayılan true)
        options.RedirectPaths.Add(401, "/Auth/Login"); // 401 için özel login yolu
        options.RedirectPaths.Add(404, "/Home/NotFoundPage"); // 404 için özel sayfa
        // options.RedirectPaths.Add(403, "/Home/AccessDenied"); // 403 için özel sayfa
    });

    var app = builder.Build();

    // ...

    // Middleware'i pipeline'a ekle
    // ÖNEMLİ: Genellikle UseRouting, UseAuthentication, UseAuthorization, UseEndpoints gibi
    // middleware'lerden SONRA, ama UseStaticFiles gibi dosya sunuculardan ÖNCE
    // veya hataları yakalamak için UseExceptionHandler'dan SONRA eklenebilir.
    // Doğru yerleşim, projenizin diğer middleware'lerine bağlıdır.
    // Hata sayfalarını (4xx, 5xx) yakalamak için genellikle endpoint'lerden hemen sonra iyi bir yerdir.
    app.UseStatusCodeHandling();

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication(); // Eğer kullanılıyorsa
    app.UseAuthorization(); // Eğer kullanılıyorsa

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    // veya app.MapRazorPages(); vb.

    app.Run();
    ```

    `Startup.cs` (.NET 5 ve öncesi):

    ```csharp
    using Core.StatusHandling.Extensions;
    using Core.StatusHandling.Models;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Diğer servisler...
            services.AddControllersWithViews();

            services.AddStatusCodeHandling(options =>
            {
                options.EnableNotifications = true;
                options.RedirectPaths.Add(401, "/Auth/Login");
                options.RedirectPaths.Add(404, "/Home/NotFoundPage");
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Diğer middleware'ler...

            app.UseRouting(); // Önce Routing

            // Status Code Handling Middleware'ini ekle
            app.UseStatusCodeHandling();

            app.UseAuthentication(); // Eğer kullanılıyorsa
            app.UseAuthorization(); // Eğer kullanılıyorsa

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
    ```

2.  **UI Bildirimlerini Gösterme (Örnek: JavaScript ile Cookie Okuma)**

    Eğer `EnableNotifications = true` ise, handler'lar `X-Notification-Message` ve `X-Notification-Type` adında cookie'ler bırakabilir. Bu cookie'leri okuyup bir toast bildirimi göstermek için istemci tarafı (JavaScript) kodu eklemeniz gerekir.

    Örnek (jQuery ve bir toast kütüphanesi varsayımıyla, örn: Toastr):

    `wwwroot/js/site.js` veya ilgili bir JS dosyasına ekleyin:

    ```javascript
    $(document).ready(function () {
        function getCookie(name) {
            const value = `; ${document.cookie}`;
            const parts = value.split(`; ${name}=`);
            if (parts.length === 2) return parts.pop().split(';').shift();
            return null; // Cookie bulunamadı
        }

        function deleteCookie(name) {
             // Cookie'yi silmek için geçmiş bir tarih ve kök dizin veriyoruz
             document.cookie = name + '=; Path=/; Expires=Thu, 01 Jan 1970 00:00:01 GMT;';
        }

        const message = getCookie("X-Notification-Message");
        const type = getCookie("X-Notification-Type"); // örn: "info", "warning", "error"

        if (message) {
            // Toastr veya başka bir bildirim kütüphanesi ile mesajı göster
            if (typeof toastr !== 'undefined') {
                 // Türüne göre farklı toastr fonksiyonlarını çağır
                 switch (type) {
                     case 'success':
                         toastr.success(decodeURIComponent(message.replace(/\+/g, ' '))); // URL decode
                         break;
                     case 'warning':
                         toastr.warning(decodeURIComponent(message.replace(/\+/g, ' ')));
                         break;
                     case 'error':
                         toastr.error(decodeURIComponent(message.replace(/\+/g, ' ')));
                         break;
                     case 'info':
                     default: // Varsayılan olarak info
                         toastr.info(decodeURIComponent(message.replace(/\+/g, ' ')));
                         break;
                 }
            } else {
                console.warn("Toastr kütüphanesi bulunamadı, bildirim gösterilemiyor:", decodeURIComponent(message.replace(/\+/g, ' ')));
                // Alternatif: alert(decodeURIComponent(message.replace(/\+/g, ' ')));
            }

            // Cookie'yi okuduktan sonra sil (sayfa yenilendiğinde tekrar göstermesin)
            deleteCookie("X-Notification-Message");
            deleteCookie("X-Notification-Type");
        }
    });
    ```

    *Not: Bu JavaScript kodunun çalışması için projenize jQuery ve bir toast bildirim kütüphanesi (örneğin Toastr.js) eklemeniz ve `_Layout.cshtml` gibi ana şablon dosyanızda bu scriptleri çağırmanız gerekir.*

## Genişletme

* **Yeni Handler Ekleme:** `IStatusCodeHandler` arayüzünü uygulayan yeni bir sınıf oluşturun ve `AddStatusCodeHandling` çağrısından *sonra* DI container'ına kaydedin:
    ```csharp
    // Program.cs veya Startup.ConfigureServices
    services.AddStatusCodeHandling(options => { /* ... */ });
    services.AddScoped<IStatusCodeHandler, YourCustomHandler>(); // Kendi handler'ınızı ekleyin
    ```
    Eğer varsayılan bir handler'ı ezmek istiyorsanız, `TryAddScoped` yerine `AddScoped` kullanın ve sizin handler'ınız varsayılandan sonra kaydedilsin. Ya da `AddStatusCodeHandling` öncesinde kaydedin.

* **Yapılandırmayı Genişletme:** `StatusCodeHandlingOptions` sınıfına yeni özellikler ekleyerek kütüphanenin davranışını daha fazla özelleştirebilirsiniz.

## Tasarım İlkeleri

* **SOLID:** Her handler tek bir sorumluluğa odaklanır (belirli durum kodlarını işlemek).
* **Clean Architecture:** Kütüphane, UI katmanından bağımsızdır (bildirim için sadece işaret bırakır).
* **Test Edilebilirlik:** Handler'lar ve Middleware, bağımlılıkları (IOptions, ILogger, diğer handler'lar) mock edilerek test edilebilir.
* **Minimal Bağımlılık:** Sadece temel ASP.NET Core paketlerine bağımlıdır.