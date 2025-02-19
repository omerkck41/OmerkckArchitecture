using System.Net.Mail;

namespace Core.Application.Mailing.Services;

public class EmailMessage
{
    public string From { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public List<EmailRecipient> Recipients { get; set; } = [];
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public bool IsImportant { get; set; } = false;
    public List<Attachment> Attachments { get; set; } = [];
}