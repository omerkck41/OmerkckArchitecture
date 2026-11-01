using System.Text;
using Amazon;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Kck.Messaging.Abstractions;
using Microsoft.Extensions.Options;

namespace Kck.Messaging.AmazonSes;

/// <summary>
/// Sends emails via Amazon Simple Email Service v2 using the AWS SDK.
/// Supports To, Cc, and Bcc recipients with display names, plain-text and HTML bodies, and attachments
/// (sent through SESv2 <c>Simple.Attachments</c>; SES limits the whole message to 40 MB).
/// </summary>
public sealed class AmazonSesEmailProvider : IEmailProvider
{
    private readonly IAmazonSimpleEmailServiceV2 _client;

    /// <summary>
    /// Initializes the provider and configures the SES client from the supplied options,
    /// using explicit credentials when provided or the default credential chain otherwise.
    /// </summary>
    public AmazonSesEmailProvider(IOptionsMonitor<AmazonSesOptions> options)
        : this(CreateClient(options.CurrentValue))
    {
    }

    /// <summary>
    /// Initializes the provider with an existing SES client, for example one registered in the container with
    /// custom credentials or endpoint.
    /// </summary>
    public AmazonSesEmailProvider(IAmazonSimpleEmailServiceV2 client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
    }

    /// <summary>
    /// Sends the given <paramref name="message"/> through Amazon SES and throws <see cref="InvalidOperationException"/> if the service returns an HTTP error.
    /// </summary>
    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var destination = new Destination
        {
            ToAddresses = FormatRecipients(message, RecipientType.To),
            CcAddresses = FormatRecipients(message, RecipientType.Cc),
            BccAddresses = FormatRecipients(message, RecipientType.Bcc)
        };

        var body = message.IsHtml
            ? new Body { Html = new Content { Data = message.Body } }
            : new Body { Text = new Content { Data = message.Body } };

        var simple = new Message
        {
            Subject = new Content { Data = message.Subject },
            Body = body
        };

        var request = new SendEmailRequest
        {
            FromEmailAddress = FormatAddress(message.FromName, message.From),
            Destination = destination,
            Content = new EmailContent { Simple = simple }
        };

        try
        {
            if (message.Attachments.Count > 0)
                simple.Attachments = await ReadAttachmentsAsync(message.Attachments, ct).ConfigureAwait(false);

            var response = await _client.SendEmailAsync(request, ct).ConfigureAwait(false);

            if ((int)response.HttpStatusCode >= 400)
                throw new InvalidOperationException(
                    $"Amazon SES returned {response.HttpStatusCode}.");
        }
        finally
        {
            // Only the buffers created here are disposed; the caller's attachment streams stay open.
            if (simple.Attachments is not null)
            {
                foreach (var attachment in simple.Attachments)
                    attachment.RawContent?.Dispose();
            }
        }
    }

    /// <summary>
    /// Formats an address for SES. Double quotes, backslashes and line breaks are removed from
    /// <paramref name="name"/> first. The result is the plain <paramref name="email"/> when the cleaned name is empty,
    /// <c>"name" &lt;email&gt;</c> when it is printable ASCII, and otherwise an RFC 2047 encoded word
    /// <c>=?utf-8?B?&lt;base64&gt;?= &lt;email&gt;</c>, which SES requires for a non-ASCII display name.
    /// </summary>
    internal static string FormatAddress(string? name, string email)
    {
        var displayName = name?.Replace("\"", string.Empty, StringComparison.Ordinal)
            .Replace("\\", string.Empty, StringComparison.Ordinal)
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", string.Empty, StringComparison.Ordinal);

        if (string.IsNullOrWhiteSpace(displayName))
            return email;

        if (displayName.All(static c => c is >= ' ' and <= '~'))
            return $"\"{displayName}\" <{email}>";

        return $"=?utf-8?B?{Convert.ToBase64String(Encoding.UTF8.GetBytes(displayName))}?= <{email}>";
    }

    private static List<string> FormatRecipients(EmailMessage message, RecipientType type) =>
        message.Recipients.Where(r => r.Type == type).Select(r => FormatAddress(r.Name, r.Email)).ToList();

    private static async Task<List<Attachment>> ReadAttachmentsAsync(
        IReadOnlyList<EmailAttachment> attachments, CancellationToken ct)
    {
        var result = new List<Attachment>(attachments.Count);
        try
        {
            foreach (var attachment in attachments)
            {
                var buffer = new MemoryStream();
                result.Add(new Attachment
                {
                    FileName = attachment.FileName,
                    ContentType = attachment.ContentType,
                    ContentDisposition = AttachmentContentDisposition.ATTACHMENT,
                    RawContent = buffer
                });

                if (attachment.Content.CanSeek)
                    attachment.Content.Position = 0;
                await attachment.Content.CopyToAsync(buffer, ct).ConfigureAwait(false);
                buffer.Position = 0;
            }

            return result;
        }
        catch
        {
            foreach (var created in result)
                await created.RawContent.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    private static AmazonSimpleEmailServiceV2Client CreateClient(AmazonSesOptions opts) =>
        string.IsNullOrEmpty(opts.AccessKey)
            ? new AmazonSimpleEmailServiceV2Client(RegionEndpoint.GetBySystemName(opts.Region))
            : new AmazonSimpleEmailServiceV2Client(opts.AccessKey, opts.SecretKey, RegionEndpoint.GetBySystemName(opts.Region));
}
