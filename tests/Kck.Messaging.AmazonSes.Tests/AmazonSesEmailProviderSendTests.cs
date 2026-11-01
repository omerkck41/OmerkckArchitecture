using System.Net;
using System.Text;
using Amazon.SimpleEmailV2;
using AwesomeAssertions;
using Kck.Messaging.Abstractions;
using Xunit;

namespace Kck.Messaging.AmazonSes.Tests;

public class AmazonSesEmailProviderSendTests
{
    private static EmailMessage CreateMessage(
        IReadOnlyList<EmailAttachment>? attachments = null,
        string? fromName = null,
        IReadOnlyList<EmailRecipient>? recipients = null) => new()
        {
            From = "shop@example.com",
            FromName = fromName,
            Subject = "Order confirmed",
            Body = "<p>Thanks</p>",
            Recipients = recipients ?? [new EmailRecipient("ada@example.com")],
            Attachments = attachments ?? []
        };

    [Fact]
    public async Task SendAsync_WithAttachments_PutsThemIntoSimpleAttachments()
    {
        var (client, fake) = FakeSesClient.Create();
        var sut = new AmazonSesEmailProvider(client);
        using var text = new MemoryStream(Encoding.UTF8.GetBytes("a-1"));
        using var pdf = new MemoryStream(Encoding.UTF8.GetBytes("pdf-bytes"));
        var message = CreateMessage(attachments:
        [
            new EmailAttachment { FileName = "note.txt", Content = text, ContentType = "text/plain" },
            new EmailAttachment { FileName = "invoice.pdf", Content = pdf, ContentType = "application/pdf" }
        ]);

        await sut.SendAsync(message);

        var attachments = fake.Requests.Should().ContainSingle().Subject.Content.Simple.Attachments;
        attachments.Should().HaveCount(2);
        attachments[0].FileName.Should().Be("note.txt");
        attachments[0].ContentType.Should().Be("text/plain");
        attachments[0].ContentDisposition.Should().Be(AttachmentContentDisposition.ATTACHMENT);
        Encoding.UTF8.GetString(attachments[0].RawContent.ToArray()).Should().Be("a-1");
        attachments[1].FileName.Should().Be("invoice.pdf");
        attachments[1].ContentType.Should().Be("application/pdf");
        attachments[1].ContentDisposition.Should().Be(AttachmentContentDisposition.ATTACHMENT);
        Encoding.UTF8.GetString(attachments[1].RawContent.ToArray()).Should().Be("pdf-bytes");
    }

    [Fact]
    public async Task SendAsync_WithAttachmentStreamNotAtStart_SendsTheWholeContent()
    {
        var (client, fake) = FakeSesClient.Create();
        var sut = new AmazonSesEmailProvider(client);
        using var text = new MemoryStream(Encoding.UTF8.GetBytes("a-1"));
        text.Position = text.Length;
        var message = CreateMessage(attachments:
            [new EmailAttachment { FileName = "note.txt", Content = text, ContentType = "text/plain" }]);

        await sut.SendAsync(message);

        var attachment = fake.Requests.Should().ContainSingle().Subject.Content.Simple.Attachments.Should().ContainSingle().Subject;
        Encoding.UTF8.GetString(attachment.RawContent.ToArray()).Should().Be("a-1");
        text.CanRead.Should().BeTrue("the caller's stream is not disposed by the provider");
    }

    [Fact]
    public async Task SendAsync_WithoutAttachments_LeavesAttachmentsUnset()
    {
        var (client, fake) = FakeSesClient.Create();
        var sut = new AmazonSesEmailProvider(client);

        await sut.SendAsync(CreateMessage());

        fake.Requests.Should().ContainSingle().Subject.Content.Simple.Attachments.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task SendAsync_WithFromNameAndRecipientNames_FormatsFriendlyAddresses()
    {
        var (client, fake) = FakeSesClient.Create();
        var sut = new AmazonSesEmailProvider(client);
        var message = CreateMessage(
            fromName: "Kck Shop",
            recipients:
            [
                new EmailRecipient("ada@example.com", "Ada"),
                new EmailRecipient("cc@example.com", Type: RecipientType.Cc),
                new EmailRecipient("audit@example.com", "Audit", RecipientType.Bcc)
            ]);

        await sut.SendAsync(message);

        var request = fake.Requests.Should().ContainSingle().Subject;
        request.FromEmailAddress.Should().Be("\"Kck Shop\" <shop@example.com>");
        request.Destination.ToAddresses.Should().Equal("\"Ada\" <ada@example.com>");
        request.Destination.CcAddresses.Should().Equal("cc@example.com");
        request.Destination.BccAddresses.Should().Equal("\"Audit\" <audit@example.com>");
    }

    [Fact]
    public async Task SendAsync_WhenSesReturnsError_Throws()
    {
        var (client, fake) = FakeSesClient.Create();
        fake.StatusCode = HttpStatusCode.BadRequest;
        var sut = new AmazonSesEmailProvider(client);

        var act = () => sut.SendAsync(CreateMessage());

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [InlineData(null, "ada@example.com")]
    [InlineData("", "ada@example.com")]
    [InlineData("   ", "ada@example.com")]
    [InlineData("Ada", "\"Ada\" <ada@example.com>")]
    [InlineData("Ada \"The\" Admin", "\"Ada The Admin\" <ada@example.com>")]
    [InlineData("Ada\r\nBcc: evil@example.com", "\"AdaBcc: evil@example.com\" <ada@example.com>")]
    [InlineData("\r\n", "ada@example.com")]
    public void FormatAddress_StripsQuotesAndLineBreaks(string? name, string expected)
    {
        AmazonSesEmailProvider.FormatAddress(name, "ada@example.com").Should().Be(expected);
    }

    // SES: a display name with non-ASCII characters must be MIME encoded-word (RFC 2047) encoded.
    [Fact]
    public void FormatAddress_WithNonAsciiName_EncodesRfc2047()
    {
        var expectedWord = Convert.ToBase64String(Encoding.UTF8.GetBytes("Ömer Küçük"));

        AmazonSesEmailProvider.FormatAddress("Ömer Küçük", "o@example.com")
            .Should().Be($"=?utf-8?B?{expectedWord}?= <o@example.com>");
    }

    // RFC 5322 quoted-string: an unescaped backslash would escape the closing quote.
    [Fact]
    public void FormatAddress_StripsBackslash()
    {
        AmazonSesEmailProvider.FormatAddress("Ad\\", "a@x.io").Should().Be("\"Ad\" <a@x.io>");
    }
}
