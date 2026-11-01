using System.Net;
using System.Reflection;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;

namespace Kck.Messaging.AmazonSes.Tests;

/// <summary>
/// Test double for <see cref="IAmazonSimpleEmailServiceV2"/> built on <see cref="DispatchProxy"/>
/// (the test project has no mocking library). It records every <c>SendEmailAsync</c> request and
/// answers with <see cref="StatusCode"/>; any other member throws <see cref="NotSupportedException"/>.
/// DispatchProxy needs a non-sealed proxy type.
/// </summary>
public class FakeSesClient : DispatchProxy
{
    /// <summary>Gets the requests passed to <c>SendEmailAsync</c>, in call order.</summary>
    public List<SendEmailRequest> Requests { get; } = [];

    /// <summary>Gets or sets the HTTP status code returned by <c>SendEmailAsync</c>.</summary>
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

    /// <summary>Creates a proxy implementing the SES client interface and returns it with its recorder.</summary>
    public static (IAmazonSimpleEmailServiceV2 Client, FakeSesClient Fake) Create()
    {
        var client = Create<IAmazonSimpleEmailServiceV2, FakeSesClient>();
        return (client, (FakeSesClient)client);
    }

    /// <inheritdoc />
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod?.Name == nameof(IAmazonSimpleEmailServiceV2.SendEmailAsync)
            && args is [SendEmailRequest request, ..])
        {
            Requests.Add(request);
            return Task.FromResult(new SendEmailResponse { HttpStatusCode = StatusCode });
        }

        if (targetMethod?.Name == nameof(IDisposable.Dispose))
            return null;

        throw new NotSupportedException($"FakeSesClient does not implement {targetMethod?.Name}.");
    }
}
