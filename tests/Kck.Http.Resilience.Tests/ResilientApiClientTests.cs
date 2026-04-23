using System.Net;
using System.Text.Json;
using FluentAssertions;
using Kck.Http.Resilience;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kck.Http.Resilience.Tests;

public class ResilientApiClientTests
{
    [Fact]
    public async Task GetAsync_Success200_ShouldReturnSuccessResponse()
    {
        var expected = new TestDto { Id = 1, Name = "Test" };
        var handler = new MockHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(expected), System.Text.Encoding.UTF8, "application/json")
            });
        var client = CreateClient(handler);

        var result = await client.GetAsync<TestDto>("https://api.test/items/1");

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetAsync_NotFound404_ShouldReturnFailure()
    {
        var handler = new MockHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("Not found")
            });
        var client = CreateClient(handler);

        var result = await client.GetAsync<TestDto>("https://api.test/items/999");

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.ErrorMessage.Should().Contain("Not found");
    }

    [Fact]
    public async Task GetAsync_Timeout_ShouldReturn408()
    {
        var handler = new MockHttpMessageHandler(
            new TaskCanceledException("Timeout", new TimeoutException()));
        var client = CreateClient(handler);

        var result = await client.GetAsync<TestDto>("https://api.test/slow");

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(408);
        result.ErrorMessage.Should().Contain("timed out");
    }

    [Fact]
    public async Task SendAsync_WithHeaders_ShouldIncludeHeaders()
    {
        HttpRequestMessage? capturedRequest = null;
        var handler = new MockHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\":1,\"name\":\"ok\"}", System.Text.Encoding.UTF8, "application/json")
            },
            req => capturedRequest = req);
        var client = CreateClient(handler);

        var headers = new Dictionary<string, string> { ["X-Custom"] = "test-value" };
        await client.SendAsync<TestDto>(HttpMethod.Get, "https://api.test/items", headers: headers);

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.GetValues("X-Custom").Should().Contain("test-value");
    }

    private static ResilientApiClient CreateClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        return new ResilientApiClient(httpClient, new NullLogger<ResilientApiClient>());
    }

    public sealed record TestDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}

internal sealed class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage? _response;
    private readonly TaskCanceledException? _exception;
    private readonly Action<HttpRequestMessage>? _onSend;

    public MockHttpMessageHandler(HttpResponseMessage response, Action<HttpRequestMessage>? onSend = null)
    {
        _response = response;
        _onSend = onSend;
    }

    public MockHttpMessageHandler(TaskCanceledException exception)
    {
        _exception = exception;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _onSend?.Invoke(request);

        if (_exception is not null)
            throw _exception;

        return Task.FromResult(_response!);
    }
}
