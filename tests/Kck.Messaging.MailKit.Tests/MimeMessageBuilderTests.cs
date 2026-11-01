using AwesomeAssertions;
using Kck.Messaging.Abstractions;
using MimeKit;
using Xunit;

namespace Kck.Messaging.MailKit.Tests;

/// <summary>The <see cref="EmailMessage"/> to <see cref="MimeMessage"/> mapping of the SMTP provider, without a server.</summary>
public sealed class MimeMessageBuilderTests
{
    private static EmailMessage Message(
        IReadOnlyList<EmailRecipient>? recipients = null,
        IReadOnlyList<EmailAttachment>? attachments = null,
        string? fromName = null,
        bool isHtml = true) => new()
        {
            From = "sender@example.com",
            FromName = fromName,
            Subject = "Subject",
            Body = isHtml ? "<p>Hello</p>" : "Hello",
            IsHtml = isHtml,
            Recipients = recipients ?? [new EmailRecipient("to@example.com")],
            Attachments = attachments ?? [],
        };

    [Fact]
    public async Task BuildMimeMessage_WithToCcBcc_PlacesEachRecipientInItsHeader()
    {
        var message = Message(recipients:
        [
            new EmailRecipient("to@example.com", "To Person"),
            new EmailRecipient("cc@example.com", Type: RecipientType.Cc),
            new EmailRecipient("bcc@example.com", Type: RecipientType.Bcc),
        ]);

        using var mime = await MailKitEmailProvider.BuildMimeMessageAsync(message, CancellationToken.None);

        mime.To.Mailboxes.Select(m => m.Address).Should().Equal("to@example.com");
        mime.To.Mailboxes.Single().Name.Should().Be("To Person");
        mime.Cc.Mailboxes.Select(m => m.Address).Should().Equal("cc@example.com");
        mime.Bcc.Mailboxes.Select(m => m.Address).Should().Equal("bcc@example.com");
        mime.Subject.Should().Be("Subject");
    }

    [Fact]
    public async Task BuildMimeMessage_WithFromName_FormatsMailbox()
    {
        using var mime = await MailKitEmailProvider.BuildMimeMessageAsync(
            Message(fromName: "Kck Team"), CancellationToken.None);

        var from = mime.From.Mailboxes.Should().ContainSingle().Subject;
        from.Address.Should().Be("sender@example.com");
        from.Name.Should().Be("Kck Team");
        from.ToString().Should().Be("\"Kck Team\" <sender@example.com>");
    }

    [Theory]
    [InlineData(true, "html")]
    [InlineData(false, "plain")]
    public async Task BuildMimeMessage_HtmlAndText_SetsMatchingBodyParts(bool isHtml, string subtype)
    {
        var message = Message(isHtml: isHtml);

        using var mime = await MailKitEmailProvider.BuildMimeMessageAsync(message, CancellationToken.None);

        var body = mime.Body.Should().BeOfType<TextPart>().Subject;
        body.ContentType.MediaSubtype.Should().Be(subtype);
        body.Text.Should().Be(message.Body);
    }

    [Fact]
    public async Task BuildMimeMessage_WithAttachment_KeepsFileNameContentTypeAndBytes()
    {
        byte[] bytes = [1, 2, 3, 4, 5];
        using var content = new MemoryStream(bytes);
        var message = Message(attachments:
            [new EmailAttachment { FileName = "report.pdf", Content = content, ContentType = "application/pdf" }]);

        using var mime = await MailKitEmailProvider.BuildMimeMessageAsync(message, CancellationToken.None);

        var part = mime.Attachments.Should().ContainSingle().Subject.Should().BeOfType<MimePart>().Subject;
        part.FileName.Should().Be("report.pdf");
        part.ContentType.MimeType.Should().Be("application/pdf");
        Decode(part).Should().Equal(bytes);
    }

    [Fact]
    public async Task BuildMimeMessage_WithAttachmentStreamNotAtStart_SendsWholeContent()
    {
        // A stream the caller has just written to is positioned at its end.
        byte[] bytes = [10, 20, 30, 40];
        using var content = new MemoryStream();
        content.Write(bytes);
        var message = Message(attachments: [new EmailAttachment { FileName = "data.bin", Content = content }]);

        using var mime = await MailKitEmailProvider.BuildMimeMessageAsync(message, CancellationToken.None);

        var part = mime.Attachments.Should().ContainSingle().Subject.Should().BeOfType<MimePart>().Subject;
        Decode(part).Should().Equal(bytes);
    }

    private static byte[] Decode(MimePart part)
    {
        part.Content.Should().NotBeNull();
        using var decoded = new MemoryStream();
        part.Content!.DecodeTo(decoded);
        return decoded.ToArray();
    }
}
