using Core.Security.EmailAuthenticator;
using Core.Security.MFA;
using Core.Security.OAuth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Core.Security;

public static class SecurityServiceRegistration
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OtpSettings>(configuration.GetSection("OtpSettings"));

        // MFA Servisleri
        services.AddSingleton<OtpSettings>();
        services.AddSingleton<IOtpService, TotpService>();
        services.AddSingleton<IMfaService, MfaService>();

        // EmailAuthenticatorHelper
        services.AddSingleton<IEmailAuthenticatorHelper, EmailAuthenticatorHelper>();


        // OAuth Servisleri
        // OAuth yapılandırmasını appsettings.json'dan yükle
        // OAuthSettings'i appsettings.json'dan yükle
        services.Configure<OAuthSettings>(configuration.GetSection("OAuthSettings"));
        // OAuthSettings'i kullanarak OAuthConfiguration'ı oluştur ve DI'ye kaydet
        services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<OAuthSettings>>().Value;
            return new OAuthConfiguration(settings);
        });
        // HttpClient ve OAuthService'i DI'ye kaydet
        services.AddHttpClient<IOAuthService, OAuthService>();

        return services;
    }
}
