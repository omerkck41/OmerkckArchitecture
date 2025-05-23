﻿using Amazon;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Core.Application.Mailing.Models;
using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Microsoft.Extensions.Options;
using System.Net;

namespace Core.Application.Mailing.Services;

public class AmazonSesEmailProvider : IEmailProvider
{
    private readonly AmazonSimpleEmailServiceV2Client _client;
    private readonly EmailSettings _emailSettings;

    public AmazonSesEmailProvider(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings?.Value ?? throw new CustomArgumentException(nameof(emailSettings));


        if (string.IsNullOrWhiteSpace(_emailSettings.DefaultFromAddress))
            throw new CustomArgumentException("DefaultFromAddress must be specified.", nameof(_emailSettings.DefaultFromAddress));

        // Amazon SES Client oluşturuluyor
        if (string.IsNullOrWhiteSpace(_emailSettings.AwsRegion) ||
            string.IsNullOrWhiteSpace(_emailSettings.AwsAccessKey) ||
            string.IsNullOrWhiteSpace(_emailSettings.AwsSecretKey))
        {
            throw new CustomArgumentException("AWS configuration is missing.");
        }

        _client = new AmazonSimpleEmailServiceV2Client(
            _emailSettings.AwsAccessKey,
            _emailSettings.AwsSecretKey,
            RegionEndpoint.GetBySystemName(_emailSettings.AwsRegion)
        );
    }

    public async Task SendAsync(EmailMessage emailMessage)
    {
        if (emailMessage == null)
            throw new CustomArgumentException(nameof(emailMessage));


        // Alıcıları hazırla
        var destination = BuildDestination(emailMessage.Recipients);

        // E-posta gönderim isteğini oluştur
        var request = BuildSendEmailRequest(emailMessage, destination);

        var response = await _client.SendEmailAsync(request);

        if (response.HttpStatusCode != HttpStatusCode.OK)
        {
            // Hata loglanabilir
            throw new CustomArgumentException("Failed to send email with Amazon SES.");
        }
    }

    /// <summary>
    /// Creates a Destination object based on the recipient information in the EmailMessage.
    /// </summary>
    private static Destination BuildDestination(IEnumerable<EmailRecipient> recipients)
    {
        if (recipients == null)
            throw new CustomArgumentException(nameof(recipients));

        return new Destination
        {
            ToAddresses = recipients.Where(r => r.Type == RecipientType.To).Select(r => r.Email).ToList(),
            CcAddresses = recipients.Where(r => r.Type == RecipientType.Cc).Select(r => r.Email).ToList(),
            BccAddresses = recipients.Where(r => r.Type == RecipientType.Bcc).Select(r => r.Email).ToList()
        };
    }

    /// <summary>
    /// Creates a SendEmailRequest object using EmailMessage data.
    /// </summary>
    private static SendEmailRequest BuildSendEmailRequest(EmailMessage emailMessage, Destination destination)
    {
        var subjectContent = new Content { Data = emailMessage.Subject };

        var bodyContent = emailMessage.IsHtml
            ? new Body { Html = new Content { Data = emailMessage.Body } }
            : new Body { Text = new Content { Data = emailMessage.Body } };

        var message = new Message
        {
            Subject = subjectContent,
            Body = bodyContent
        };

        return new SendEmailRequest
        {
            FromEmailAddress = emailMessage.From,
            Destination = destination,
            Content = new EmailContent
            {
                Simple = message
            },
            ReplyToAddresses = [$"{emailMessage.FromName} <{emailMessage.From}>"]
        };
    }
}