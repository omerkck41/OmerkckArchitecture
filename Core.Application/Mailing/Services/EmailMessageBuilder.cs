using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Core.Application.Mailing.Services;

public class EmailMessageBuilder
{
    private readonly EmailMessage _emailMessage = new();

    public EmailMessageBuilder AddFrom(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("From address cannot be empty.", nameof(address));

        if (!IsValidEmail(address))
            throw new FormatException("Invalid email format.");

        _emailMessage.From = address;
        return this;
    }

    public EmailMessageBuilder AddRecipient(string name, string email, RecipientType type)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        if (!IsValidEmail(email))
            throw new FormatException("Invalid email format.");

        _emailMessage.Recipients.Add(new EmailRecipient { Name = name, Email = email, Type = type });
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

    private bool IsValidEmail(string email)
    {
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        return emailRegex.IsMatch(email);
    }
}