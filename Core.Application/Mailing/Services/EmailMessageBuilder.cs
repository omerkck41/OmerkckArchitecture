using System.Net.Mail;

namespace Core.Application.Mailing.Services;

public class EmailMessageBuilder
{
    private readonly EmailMessage _emailMessage = new();

    public EmailMessageBuilder AddFrom(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("From address cannot be empty.", nameof(address));

        _emailMessage.From = address;
        return this;
    }

    public EmailMessageBuilder AddTo(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("To address cannot be empty.", nameof(address));

        _emailMessage.To.Add(address);
        return this;
    }

    public EmailMessageBuilder AddCc(string address)
    {
        _emailMessage.Cc.Add(address);
        return this;
    }

    public EmailMessageBuilder AddBcc(string address)
    {
        _emailMessage.Bcc.Add(address);
        return this;
    }

    public EmailMessageBuilder AddSubject(string subject)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject cannot be empty.", nameof(subject));

        _emailMessage.Subject = subject;
        return this;
    }

    public EmailMessageBuilder AddBody(string body, bool isHtml = true)
    {
        _emailMessage.Body = body;
        _emailMessage.IsHtml = isHtml;
        return this;
    }

    public EmailMessageBuilder MarkAsImportant()
    {
        _emailMessage.IsImportant = true;
        return this;
    }

    public EmailMessageBuilder AddAttachment(Attachment attachment)
    {
        _emailMessage.Attachments.Add(attachment);
        return this;
    }

    public EmailMessage Build() => _emailMessage;
}