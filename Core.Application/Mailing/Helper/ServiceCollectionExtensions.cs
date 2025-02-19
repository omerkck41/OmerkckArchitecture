using Core.Application.Mailing.Models;
using Core.Application.Mailing.Services;
using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace Core.Application.Mailing.Helper;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// IOptions pattern kullanarak, JSON'daki "EmailSettings" bölümünü register edip mailing servislerini kaydeder.
    /// </summary>
    public static IServiceCollection AddMailingServicesFromJson(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration == null)
            throw new CustomException(nameof(configuration));

        // IOptions pattern ile EmailSettings strongly-typed olarak register ediliyor
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        return AddMailingServices(services);
    }

    /// <summary>
    /// IOptions pattern kullanarak, verilen EmailSettings nesnesini register edip mailing servislerini kaydeder.
    /// </summary>
    public static IServiceCollection AddMailingServicesFromObject(this IServiceCollection services, EmailSettings emailSettings)
    {
        if (emailSettings == null)
            throw new CustomException(nameof(emailSettings), "EmailSettings cannot be null.");

        // Options.Create ile EmailSettings'i IOptions olarak register ediyoruz
        services.AddSingleton(Options.Create(emailSettings));

        return AddMailingServices(services);
    }

    /// <summary>
    /// Ortak servis kayıt metodudur. Bu metotta mailing servisleri, sağlayıcılar ve SMTP istemcisi kaydedilir.
    /// </summary>
    private static IServiceCollection AddMailingServices(IServiceCollection services)
    {
        // E-posta sağlayıcılarını kaydediyoruz
        services.AddTransient<IEmailProvider, SmtpEmailProvider>();
        services.AddTransient<IEmailProvider, SendGridEmailProvider>();
        services.AddTransient<IEmailProvider, AmazonSesEmailProvider>();

        // E-posta gönderim servisini kaydediyoruz. IOptions<EmailSettings> üzerinden ayarlar DI'dan çekiliyor.
        services.AddTransient<IMailService, EmailSendingService>(provider =>
        {
            var emailProviders = provider.GetServices<IEmailProvider>();
            var emailSettings = provider.GetRequiredService<IOptions<EmailSettings>>().Value;
            return new EmailSendingService(emailProviders, emailSettings);
        });

        // SMTP istemci seçiciyi kaydediyoruz (örneğin RateLimitingSmtpClientSelector kullanarak)
        services.AddSingleton<ISmtpClientSelector, RateLimitingSmtpClientSelector>(provider =>
        {
            var emailSettings = provider.GetRequiredService<IOptions<EmailSettings>>().Value;
            var smtpClients = emailSettings.SmtpServers.Select(server => new SmtpClient(server.Host)
            {
                Port = server.Port,
                Credentials = new System.Net.NetworkCredential(server.Username, server.Password),
                EnableSsl = server.UseSsl
            }).ToList();

            return new RateLimitingSmtpClientSelector(smtpClients, emailSettings.MaxSendsPerClient);
        });

        // Logging servisini ekliyoruz (eğer henüz eklenmediyse)
        services.AddLogging();

        return services;
    }
}