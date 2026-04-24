using Kck.Messaging.Abstractions;
using MimeKit;

namespace Kck.Messaging.MailKit;

public sealed class MailKitEmailProvider(SmtpConnectionPool pool) : IEmailProvider
{
    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var mime = new MimeMessage();
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
            builder.Attachments.Add(att.FileName, att.Content, ContentType.Parse(att.ContentType), ct);

        mime.Body = builder.ToMessageBody();

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
}
