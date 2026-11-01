using System.Net;
using AwesomeAssertions;
using Kck.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SendGrid;
using SendGrid.Helpers.Mail;
using Xunit;

namespace Kck.Messaging.SendGrid.Tests;

/// <summary>What the provider hands to SendGrid, through a substituted <see cref="ISendGridClient"/>.</summary>
public sealed class SendGridEmailProviderTests
{
    private readonly ISendGridClient _client = Substitute.For<ISendGridClient>();
    private SendGridMessage? _sent;

    public SendGridEmailProviderTests()
    {
        Respond(HttpStatusCode.Accepted);
    }

    private void Respond(HttpStatusCode status) =>
        _client.SendEmailAsync(Arg.Do<SendGridMessage>(m => _sent = m), Arg.Any<CancellationToken>())
            .Returns(new Response(status, null, null));

    private static EmailMessage Message(
        IReadOnlyList<EmailRecipient>? recipients = null,
        IReadOnlyList<EmailAttachment>? attachments = null,
        bool isHtml = true) => new()
        {
            From = "sender@example.com",
            FromName = "Sender",
            Subject = "Subject",
            Body = isHtml ? "<p>Hello</p>" : "Hello",
            IsHtml = isHtml,
            Recipients = recipients ?? [new EmailRecipient("to@example.com")],
            Attachments = attachments ?? [],
        };

    [Fact]
    public async Task SendAsync_MapsToCcBccIntoPersonalization()
    {
        var sut = new SendGridEmailProvider(_client);

        await sut.SendAsync(Message(recipients:
        [
            new EmailRecipient("to@example.com", "To Person"),
            new EmailRecipient("cc@example.com", Type: RecipientType.Cc),
            new EmailRecipient("bcc@example.com", Type: RecipientType.Bcc),
        ]));

        var personalization = _sent!.Personalizations.Should().ContainSingle().Subject;
        personalization.Tos.Select(a => a.Email).Should().Equal("to@example.com");
        personalization.Tos.Single().Name.Should().Be("To Person");
        personalization.Ccs.Select(a => a.Email).Should().Equal("cc@example.com");
        personalization.Bccs.Select(a => a.Email).Should().Equal("bcc@example.com");
        _sent.From.Email.Should().Be("sender@example.com");
        _sent.From.Name.Should().Be("Sender");
    }

    [Fact]
    public async Task SendAsync_WithAttachments_Base64EncodesWithNameAndType()
    {
        byte[] bytes = [1, 2, 3, 250];
        using var content = new MemoryStream(bytes);
        var sut = new SendGridEmailProvider(_client);

        await sut.SendAsync(Message(attachments:
            [new EmailAttachment { FileName = "report.pdf", Content = content, ContentType = "application/pdf" }]));

        var attachment = _sent!.Attachments.Should().ContainSingle().Subject;
        attachment.Filename.Should().Be("report.pdf");
        attachment.Type.Should().Be("application/pdf");
        attachment.Content.Should().Be(Convert.ToBase64String(bytes));
    }

    [Fact]
    public async Task SendAsync_AttachmentStreamNotAtStart_SendsWholeContent()
    {
        byte[] bytes = [10, 20, 30];
        using var content = new MemoryStream();
        content.Write(bytes);
        var sut = new SendGridEmailProvider(_client);

        await sut.SendAsync(Message(attachments: [new EmailAttachment { FileName = "data.bin", Content = content }]));

        _sent!.Attachments.Should().ContainSingle().Which.Content.Should().Be(Convert.ToBase64String(bytes));
    }

    [Fact]
    public async Task SendAsync_HtmlBody_SetsHtmlContent()
    {
        var sut = new SendGridEmailProvider(_client);

        await sut.SendAsync(Message(isHtml: true));

        // MailHelper writes the bodies into Contents (serialized as "content"), not into HtmlContent.
        var content = _sent!.Contents.Should().ContainSingle().Subject;
        content.Type.Should().Be(MimeType.Html);
        content.Value.Should().Be("<p>Hello</p>");
    }

    [Fact]
    public async Task SendAsync_NonSuccessStatus_ThrowsInvalidOperationException()
    {
        Respond(HttpStatusCode.BadRequest);
        var sut = new SendGridEmailProvider(_client);

        var send = () => sut.SendAsync(Message());

        await send.Should().ThrowAsync<InvalidOperationException>().WithMessage("*BadRequest*");
    }

    [Fact]
    public void ActivatorUtilities_WithClientAndOptionsRegistered_PicksTheOptionsConstructor()
    {
        // Both public constructors take one resolvable service; the options one is marked as the activator's choice
        // (the 4.x constructor), so a manual registration is not ambiguous.
        var services = new ServiceCollection();
        services.AddSingleton(_client);
        services.Configure<SendGridOptions>(o => o.ApiKey = "SG.test");
        using var provider = services.BuildServiceProvider();

        var create = () => ActivatorUtilities.CreateInstance<SendGridEmailProvider>(provider);

        create.Should().NotThrow().Which.Should().BeOfType<SendGridEmailProvider>();
    }

    [Fact]
    public void AddSingleton_WithClientAndOptionsRegistered_IsAmbiguousForTheContainer()
    {
        // Known limit, pinned: the Microsoft.Extensions.DependencyInjection container ignores
        // [ActivatorUtilitiesConstructor] (only ActivatorUtilities honours it), so a plain type registration with both
        // an ISendGridClient and the options registered cannot choose. AddKckMessagingSendGrid registers a factory and
        // is the supported path.
        var services = new ServiceCollection();
        services.AddSingleton(_client);
        services.Configure<SendGridOptions>(o => o.ApiKey = "SG.test");
        services.AddSingleton<IEmailProvider, SendGridEmailProvider>();
        using var provider = services.BuildServiceProvider();

        var resolve = () => provider.GetRequiredService<IEmailProvider>();

        resolve.Should().Throw<InvalidOperationException>().WithMessage("*constructors are ambiguous*");
    }

    [Fact]
    public async Task AddKckMessagingSendGrid_WithRegisteredClient_UsesIt()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_client);
        services.AddKckMessagingSendGrid(_ => { });

        await using var provider = services.BuildServiceProvider(validateScopes: true);
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<SendGridOptions>>();

        // No ApiKey: the validator accepts it because a client is registered.
        var resolveOptions = () => options.CurrentValue;
        resolveOptions.Should().NotThrow();
        await provider.GetRequiredService<IEmailProvider>().SendAsync(Message());
        _sent.Should().NotBeNull("the registered client must be the one that sends");
    }
}
