using Amazon;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Kck.Messaging.Abstractions;
using Microsoft.Extensions.Options;

namespace Kck.Messaging.AmazonSes;

/// <summary>
/// Sends emails via Amazon Simple Email Service v2 using the AWS SDK.
/// Supports To, Cc, and Bcc recipients as well as plain-text and HTML bodies.
/// </summary>
public sealed class AmazonSesEmailProvider : IEmailProvider
{
    private readonly AmazonSimpleEmailServiceV2Client _client;

    /// <summary>
    /// Initializes the provider and configures the SES client from the supplied options,
    /// using explicit credentials when provided or the default credential chain otherwise.
    /// </summary>
    public AmazonSesEmailProvider(IOptionsMonitor<AmazonSesOptions> options)
    {
        var opts = options.CurrentValue;
        _client = string.IsNullOrEmpty(opts.AccessKey)
            ? new AmazonSimpleEmailServiceV2Client(RegionEndpoint.GetBySystemName(opts.Region))
            : new AmazonSimpleEmailServiceV2Client(opts.AccessKey, opts.SecretKey, RegionEndpoint.GetBySystemName(opts.Region));
    }

    /// <summary>
    /// Sends the given <paramref name="message"/> through Amazon SES and throws <see cref="InvalidOperationException"/> if the service returns an HTTP error.
    /// </summary>
    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var destination = new Destination
        {
            ToAddresses = message.Recipients.Where(r => r.Type == RecipientType.To).Select(r => r.Email).ToList(),
            CcAddresses = message.Recipients.Where(r => r.Type == RecipientType.Cc).Select(r => r.Email).ToList(),
            BccAddresses = message.Recipients.Where(r => r.Type == RecipientType.Bcc).Select(r => r.Email).ToList()
        };

        var body = message.IsHtml
            ? new Body { Html = new Content { Data = message.Body } }
            : new Body { Text = new Content { Data = message.Body } };

        var request = new SendEmailRequest
        {
            FromEmailAddress = message.From,
            Destination = destination,
            Content = new EmailContent
            {
                Simple = new Message
                {
                    Subject = new Content { Data = message.Subject },
                    Body = body
                }
            }
        };

        var response = await _client.SendEmailAsync(request, ct).ConfigureAwait(false);

        if ((int)response.HttpStatusCode >= 400)
            throw new InvalidOperationException(
                $"Amazon SES returned {response.HttpStatusCode}.");
    }
}
