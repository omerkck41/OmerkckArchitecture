using Kck.Messaging.Abstractions;
using MimeKit;

namespace Kck.Messaging.MailKit;

/// <summary>
/// Sends emails via SMTP using MailKit, borrowing connections from a shared <see cref="SmtpConnectionPool"/>.
/// </summary>
public sealed class MailKitEmailProvider(SmtpConnectionPool pool) : IEmailProvider
{
    /// <summary>
    /// Builds a MIME message from <paramref name="message"/> and transmits it over a pooled SMTP connection.
    /// </summary>
    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        using var mime = await BuildMimeMessageAsync(message, ct).ConfigureAwait(false);

        var client = await pool.RentAsync(ct).ConfigureAwait(false);
        try
        {
            await client.SendAsync(mime, ct).ConfigureAwait(false);
        }
        finally
        {
            await pool.ReturnAsync(client).ConfigureAwait(false);
        }
    }

    /// <summary>Maps <paramref name="message"/> to the MIME message that is sent; the caller disposes it.</summary>
    internal static async Task<MimeMessage> BuildMimeMessageAsync(EmailMessage message, CancellationToken ct)
    {
        var mime = new MimeMessage();
        try
        {
            await FillAsync(mime, message, ct).ConfigureAwait(false);
            return mime;
        }
        catch
        {
            mime.Dispose();
            throw;
        }
    }

    private static async Task FillAsync(MimeMessage mime, EmailMessage message, CancellationToken ct)
    {
        mime.From.Add(new MailboxAddress(message.FromName ?? message.From, message.From));

        foreach (var r in message.Recipients)
        {
            var addr = new MailboxAddress(r.Name ?? r.Email, r.Email);
            switch (r.Type)
            {
                case RecipientType.To:  mime.To.Add(addr); break;
                case RecipientType.Cc:  mime.Cc.Add(addr); break;
                case RecipientType.Bcc: mime.Bcc.Add(addr); break;
            }
        }

        mime.Subject = message.Subject;

        var builder = new BodyBuilder();
        if (message.IsHtml)
            builder.HtmlBody = message.Body;
        else
            builder.TextBody = message.Body;

        foreach (var att in message.Attachments)
        {
            // MimeKit copies from the current position: a stream the caller has just written is at its end.
            if (att.Content.CanSeek)
                att.Content.Position = 0;

            await builder.Attachments.AddAsync(att.FileName, att.Content, ContentType.Parse(att.ContentType), ct).ConfigureAwait(false);
        }

        mime.Body = builder.ToMessageBody();
    }
}
