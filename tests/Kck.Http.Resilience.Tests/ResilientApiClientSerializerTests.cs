using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using Kck.Http.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kck.Http.Resilience.Tests;

// Native AOT apps pass source-generated JSON metadata instead of relying on reflection.
public sealed partial class ResilientApiClientSerializerTests
{
    public sealed record OrderRequest(int OrderId);

    public sealed record OrderResult(int TotalCount);

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
    [JsonSerializable(typeof(OrderRequest))]
    [JsonSerializable(typeof(OrderResult))]
    internal sealed partial class OrderJsonContext : JsonSerializerContext;

    [Fact]
    public async Task SendAsync_WithSerializerOptions_WritesAndReadsThroughThem()
    {
        using var handler = new CapturingHandler("""{"total_count":3}""");
        using var httpClient = new HttpClient(handler);
        var sut = new ResilientApiClient(
            httpClient,
            NullLogger<ResilientApiClient>.Instance,
            Options.Create(new ResilienceOptions { SerializerOptions = OrderJsonContext.Default.Options }));

        var result = await sut.PostAsync<OrderResult>("https://api.test/orders", new OrderRequest(7));

        handler.Body.Should().Be("""{"order_id":7}""");
        result.Data.Should().Be(new OrderResult(3));
    }

    [Fact]
    public async Task SendAsync_WithoutOptions_KeepsTheWebDefaults()
    {
        using var handler = new CapturingHandler("""{"totalCount":3}""");
        using var httpClient = new HttpClient(handler);
        var sut = new ResilientApiClient(httpClient, NullLogger<ResilientApiClient>.Instance);

        var result = await sut.PostAsync<OrderResult>("https://api.test/orders", new OrderRequest(7));

        handler.Body.Should().Be("""{"orderId":7}""");
        result.Data.Should().Be(new OrderResult(3));
    }

    [Fact]
    public void AddKckHttpResilience_SerializerOptions_ReachTheTypedClient()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddKckHttpResilience(o => o.SerializerOptions = OrderJsonContext.Default.Options);

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IOptions<ResilienceOptions>>().Value.SerializerOptions
            .Should().BeSameAs(OrderJsonContext.Default.Options);
        provider.GetRequiredService<IApiClient>().Should().BeOfType<ResilientApiClient>();
    }

    private sealed class CapturingHandler(string response) : HttpMessageHandler
    {
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response, new MediaTypeHeaderValue("application/json")),
            };
        }
    }
}
