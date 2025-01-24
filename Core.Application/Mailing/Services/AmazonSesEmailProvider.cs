using Amazon;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Microsoft.Extensions.Configuration;

namespace Core.Application.Mailing.Services;

public class AmazonSesEmailProvider : IEmailProvider
{
    private readonly AmazonSimpleEmailServiceV2Client _client;

    public AmazonSesEmailProvider(IConfiguration configuration)
    {
        // AWS kimlik bilgilerini ve bölgesini yapılandırmadan al
        var awsRegion = configuration["AWS:Region"];
        var awsAccessKey = configuration["AWS:AccessKey"];
        var awsSecretKey = configuration["AWS:SecretKey"];

        // AmazonSimpleEmailServiceV2Client oluştur
        _client = new AmazonSimpleEmailServiceV2Client(
            awsAccessKey,
            awsSecretKey,
            RegionEndpoint.GetBySystemName(awsRegion)
        );
    }

    public async Task SendAsync(EmailMessage emailMessage)
    {
        // Alıcıları hazırla
        var destination = new Destination
        {
            ToAddresses = emailMessage.Recipients
                .Where(r => r.Type == RecipientType.To)
                .Select(r => r.Email)
                .ToList(),
            CcAddresses = emailMessage.Recipients
                .Where(r => r.Type == RecipientType.Cc)
                .Select(r => r.Email)
                .ToList(),
            BccAddresses = emailMessage.Recipients
                .Where(r => r.Type == RecipientType.Bcc)
                .Select(r => r.Email)
                .ToList()
        };

        // E-posta konusu ve içeriği
        var subject = new Content
        {
            Data = emailMessage.Subject
        };

        var body = new Body
        {
            Html = emailMessage.IsHtml ? new Content { Data = emailMessage.Body } : null,
            Text = emailMessage.IsHtml ? null : new Content { Data = emailMessage.Body }
        };

        // E-posta mesajı
        var message = new Message
        {
            Subject = subject,
            Body = body
        };

        // E-posta gönderme isteği
        var request = new SendEmailRequest
        {
            FromEmailAddress = emailMessage.From,
            Destination = destination,
            Content = new EmailContent
            {
                Simple = message
            }
        };

        // E-postayı gönder
        var response = await _client.SendEmailAsync(request);

        // Başarı durumunu kontrol et
        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new Exception("Failed to send email via Amazon SES.");
        }
    }
}