using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Core.Application.Mailing.Services;

public class EmailMessageBuilder
{
    private readonly EmailMessage _emailMessage = new();

    public EmailMessageBuilder AddFrom(string address, string? name = null)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new CustomException("From address cannot be empty.", nameof(address));

        if (!IsValidEmail(address))
            throw new CustomException("Invalid email format.");

        _emailMessage.From = address;
        _emailMessage.FromName = name ?? "";
        return this;
    }

    public EmailMessageBuilder AddRecipient(string email, RecipientType type, string? name = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new CustomException("Email cannot be empty.", nameof(email));

        if (!IsValidEmail(email))
            throw new CustomException("Invalid email format.");

        _emailMessage.Recipients.Add(new EmailRecipient { Name = name ?? string.Empty, Email = email, Type = type });
        return this;
    }


    public EmailMessageBuilder AddSubject(string subject)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new CustomException("Subject cannot be empty.", nameof(subject));

        _emailMessage.Subject = subject;
        return this;
    }

    public EmailMessageBuilder AddBody(string body, bool isHtml = true)
    {
        _emailMessage.Body = body;
        _emailMessage.IsHtml = isHtml;
        return this;
    }

    public EmailMessageBuilder MarkAsImportant(bool isImportant = true)
    {
        _emailMessage.IsImportant = isImportant;
        return this;
    }

    public EmailMessageBuilder AddAttachment(List<Attachment> attachments)
    {
        _emailMessage.Attachments.AddRange(attachments);
        return this;
    }

    public EmailMessage Build() => _emailMessage;

    private bool IsValidEmail(string email)
    {
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        return emailRegex.IsMatch(email);
    }
}