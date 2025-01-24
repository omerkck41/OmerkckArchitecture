using Core.Application.Mailing.Models;
using Core.Application.Mailing.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Mail;

namespace Core.Application.Mailing.Helper;

public static class ServiceCollectionExtensions
{
    // JSON'dan bilgileri alarak servisleri kaydeden metot
    public static IServiceCollection AddMailingServicesFromJson(this IServiceCollection services, IConfiguration configuration)
    {
        // JSON'dan e-posta ayarlarını oku
        var emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>();

        if (emailSettings == null)
            throw new InvalidOperationException("EmailSettings configuration section is missing or invalid.");

        // Servisleri kaydet
        return AddMailingServices(services, emailSettings);
    }

    // Bir sınıf üzerinden bilgileri alarak servisleri kaydeden metot
    public static IServiceCollection AddMailingServicesFromObject(this IServiceCollection services, EmailSettings emailSettings)
    {
        if (emailSettings == null)
            throw new ArgumentNullException(nameof(emailSettings), "EmailSettings cannot be null.");

        // Servisleri kaydet
        return AddMailingServices(services, emailSettings);
    }

    // Ortak servis kayıt metodu
    private static IServiceCollection AddMailingServices(IServiceCollection services, EmailSettings emailSettings)
    {
        // SMTP ve SendGrid gibi e-posta sağlayıcılarını kaydet
        services.AddTransient<IEmailProvider, SmtpEmailProvider>();
        services.AddTransient<IEmailProvider, SendGridEmailProvider>();

        // E-posta gönderme servisini kaydet
        services.AddTransient<IMailService, EmailSendingService>();

        // SMTP istemci seçiciyi kaydet (örnek olarak RateLimitingSmtpClientSelector kullanıyoruz)
        services.AddSingleton<ISmtpClientSelector, RateLimitingSmtpClientSelector>(provider =>
        {
            var smtpClients = emailSettings.SmtpServers.Select(server => new SmtpClient(server.Host)
            {
                Port = server.Port,
                Credentials = new System.Net.NetworkCredential(server.Username, server.Password),
                EnableSsl = server.UseSsl
            }).ToList();

            return new RateLimitingSmtpClientSelector(smtpClients, maxSendsPerClient: emailSettings.MaxSendsPerClient);
        });

        // Logging servisini kaydet (eğer yoksa)
        services.AddLogging();

        return services;
    }
}