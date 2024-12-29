using System.Net.Mail;

namespace Core.Application.Mailing.Services;

public class EmailMessage
{
    public string From { get; set; } = string.Empty;
    public List<string> To { get; set; } = [];
    public List<string> Cc { get; set; } = [];
    public List<string> Bcc { get; set; } = [];
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public bool IsImportant { get; set; } = false; // Yeni özellik
    public List<Attachment> Attachments { get; set; } = [];
}