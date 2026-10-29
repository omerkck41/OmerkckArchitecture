using System.Net;
using AwesomeAssertions;
using Kck.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kck.Http.Resilience.Tests;

public sealed class ResilientApiClientRequestTests : IDisposable
{
    private readonly List<HttpClient> _httpClients = [];

    public void Dispose()
    {
        foreach (var c in _httpClients) c.Dispose();
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("DELETE")]
    public async Task GetAndDeleteAsync_Success_UseMatchingMethodAndSendNoBody(string method)
    {
        using var handler = new RecordingHandler(HttpStatusCode.OK, "{\"id\":7,\"name\":\"x\"}");
        var client = CreateClient(handler);

        var result = method == "GET"
            ? await client.GetAsync<Dto>("https://api.test/items/7")
            : await client.DeleteAsync<Dto>("https://api.test/items/7");

        handler.Method.Should().Be(new HttpMethod(method));
        handler.Url.Should().Be("https://api.test/items/7");
        handler.Body.Should().BeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Id.Should().Be(7);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    public async Task PostAndPutAsync_WithBody_SendJsonBody(string method)
    {
        using var handler = new RecordingHandler(HttpStatusCode.Created, "{\"id\":1,\"name\":\"created\"}");
        var client = CreateClient(handler);
        var body = new Dto { Id = 1, Name = "created" };

        var result = method == "POST"
            ? await client.PostAsync<Dto>("https://api.test/items", body)
            : await client.PutAsync<Dto>("https://api.test/items", body);

        handler.Method.Should().Be(new HttpMethod(method));
        handler.ContentType.Should().Be("application/json");
        handler.Body.Should().Contain("\"name\":\"created\"");
        result.StatusCode.Should().Be(201);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task PostAsync_WithoutBody_SendsNoContent()
    {
        using var handler = new RecordingHandler(HttpStatusCode.OK, "{\"id\":1,\"name\":\"a\"}");
        var client = CreateClient(handler);

        await client.PostAsync<Dto>("https://api.test/items");

        handler.Method.Should().Be(HttpMethod.Post);
        handler.Body.Should().BeNull();
    }

    [Fact]
    public async Task SendAsync_ServerError_ReturnsFailureWithStatusAndBodyAndLogsWarning()
    {
        using var handler = new RecordingHandler(HttpStatusCode.InternalServerError, "boom");
        var logger = new ListLogger();
        var client = CreateClient(handler, logger);

        var result = await client.SendAsync<Dto>(HttpMethod.Post, "https://api.test/x", new Dto());

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(500);
        result.ErrorMessage.Should().Be("boom");
        result.Data.Should().BeNull();
        logger.Entries.Should().ContainSingle(e => e.Level == LogLevel.Warning
            && e.Message.Contains("500") && e.Message.Contains("boom") && e.Message.Contains("https://api.test/x"));
    }

    [Fact]
    public async Task SendAsync_Timeout_LogsTimeoutWarning()
    {
        var logger = new ListLogger();
        using var handler = new MockHttpMessageHandler(new TaskCanceledException("t", new TimeoutException()));
        var client = CreateClient(handler, logger);

        var result = await client.GetAsync<Dto>("https://api.test/slow");

        result.StatusCode.Should().Be(408);
        logger.Entries.Should().ContainSingle(e => e.Level == LogLevel.Warning && e.Message.Contains("timed out"));
    }

    [Fact]
    public async Task SendAsync_CallerCancelled_PropagatesInsteadOfReturning408()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        using var handler = new MockHttpMessageHandler(new TaskCanceledException("cancelled", null, cts.Token));
        var client = CreateClient(handler);

        var act = () => client.GetAsync<Dto>("https://api.test/x", cts.Token);

        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public async Task SendAsync_MultipleHeaders_AddsAllHeaders()
    {
        using var handler = new RecordingHandler(HttpStatusCode.OK, "{\"id\":1,\"name\":\"a\"}");
        var client = CreateClient(handler);
        var headers = new Dictionary<string, string> { ["X-One"] = "1", ["Authorization"] = "Bearer t" };

        await client.SendAsync<Dto>(HttpMethod.Get, "https://api.test/x", headers: headers);

        handler.Headers.Should().Contain(h => h.Key == "X-One" && h.Value.Contains("1"));
        handler.Headers.Should().Contain(h => h.Key == "Authorization" && h.Value.Contains("Bearer t"));
    }

    private ResilientApiClient CreateClient(HttpMessageHandler handler, ILogger<ResilientApiClient>? logger = null)
    {
        var httpClient = new HttpClient(handler);
        _httpClients.Add(httpClient);
        return new ResilientApiClient(httpClient, logger ?? new NullLogger<ResilientApiClient>());
    }

    private sealed record Dto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    private sealed class RecordingHandler(HttpStatusCode status, string content) : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }
        public string? Url { get; private set; }
        public string? Body { get; private set; }
        public string? ContentType { get; private set; }
        public List<KeyValuePair<string, IEnumerable<string>>> Headers { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Method = request.Method;
            Url = request.RequestUri?.ToString();
            Headers.AddRange(request.Headers);
            if (request.Content is not null)
            {
                Body = await request.Content.ReadAsStringAsync(cancellationToken);
                ContentType = request.Content.Headers.ContentType?.MediaType;
            }

            return new HttpResponseMessage(status)
            {
                Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
            };
        }
    }

    private sealed class ListLogger : ILogger<ResilientApiClient>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            Entries.Add((logLevel, formatter(state, exception)));
    }
}
