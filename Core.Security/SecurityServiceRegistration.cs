using Core.Security.EmailAuthenticator;
using Core.Security.MFA;
using Core.Security.OAuth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        var oauthConfig = configuration.GetSection("OAuthSettings").Get<OAuthConfiguration>();
        // HttpClient ve OAuthService'i dependency injection'a ekle
        services.AddHttpClient();
        services.AddSingleton(oauthConfig);
        services.AddScoped<OAuthService>();

        return services;
    }
}
