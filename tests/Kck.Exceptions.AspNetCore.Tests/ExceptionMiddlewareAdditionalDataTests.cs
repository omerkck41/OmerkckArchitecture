using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using Kck.Exceptions;
using Kck.Exceptions.AspNetCore.Handlers;
using Kck.Exceptions.AspNetCore.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kck.Exceptions.AspNetCore.Tests;

/// <summary>
/// <see cref="CustomException.AdditionalData"/> is written flat into the problem body, like RFC 9457 extension members.
/// The assertions read the raw JSON the middleware writes.
/// </summary>
public class ExceptionMiddlewareAdditionalDataTests
{
    [Fact]
    public async Task InvokeAsync_AnonymousAdditionalData_WritesPropertiesFlatWithCamelCaseKeys()
    {
        var body = await RunAsync(new BadRequestException("x", new { Field = "Email" }));

        body.Should().Contain("\"field\":\"Email\"");
        body.Should().NotContain("\"extensions\"");
    }

    [Fact]
    public async Task InvokeAsync_CustomArgumentExceptionWithParamName_WritesParamName()
    {
        var body = await RunAsync(new CustomArgumentException("bad", "email"));

        body.Should().Contain("\"paramName\":\"email\"");
    }

    [Fact]
    public async Task InvokeAsync_NoAdditionalData_WritesOnlyTheStandardMembers()
    {
        var body = await RunAsync(new BadRequestException("x"));

        using var document = JsonDocument.Parse(body);
        document.RootElement.EnumerateObject().Select(p => p.Name).Should()
            .BeEquivalentTo(["type", "title", "status", "detail", "instance", "traceId"]);
        body.Should().NotContain("null");
    }

    [Fact]
    public async Task InvokeAsync_DictionaryWithStandardMemberKey_SkipsTheCollidingKey()
    {
        var data = new Dictionary<string, object?> { ["Status"] = 999, ["status"] = 998, ["code"] = "E42" };

        var body = await RunAsync(new BadRequestException("x", data));

        using var document = JsonDocument.Parse(body);
        var names = document.RootElement.EnumerateObject().Select(p => p.Name).ToList();
        names.Count(n => string.Equals(n, "status", StringComparison.OrdinalIgnoreCase)).Should().Be(1);
        document.RootElement.GetProperty("status").GetInt32().Should().Be(400);
        document.RootElement.GetProperty("code").GetString().Should().Be("E42");
    }

    [Fact]
    public async Task InvokeAsync_NonObjectAdditionalData_WritesItUnderAdditionalDataKey()
    {
        var body = await RunAsync(new BadRequestException("x", "plain text"));

        body.Should().Contain("\"additionalData\":\"plain text\"");
    }

    [Fact]
    public async Task InvokeAsync_SelfReferencingAdditionalData_WritesProblemWithoutExtensionsAndLogsWarning()
    {
        var node = new Node { Name = "loop" };
        node.Next = node;
        var log = new List<(LogLevel Level, string Message)>();

        var body = await RunAsync(new BadRequestException("x", node), log);

        AssertStandardMembersOnly(body);
        log.Should().Contain(entry => entry.Level == LogLevel.Warning && entry.Message.Contains(nameof(JsonException)));
        log.Should().NotContain(entry => entry.Message.Contains("loop"), "the data itself is not logged");
    }

    [Fact]
    public async Task InvokeAsync_DictionaryValueNotSerializable_WritesProblemWithoutExtensions()
    {
        var data = new Dictionary<string, object?> { ["code"] = "E42", ["clrType"] = typeof(string) };

        var body = await RunAsync(new BadRequestException("x", data));

        AssertStandardMembersOnly(body);
    }

    [Fact]
    public async Task InvokeAsync_PairWithNullKey_SkipsItAndWritesTheOthers()
    {
        var data = new List<KeyValuePair<string, object?>> { new(null!, "lost"), new("code", "E42") };

        var body = await RunAsync(new BadRequestException("x", data));

        using var document = JsonDocument.Parse(body);
        document.RootElement.GetProperty("status").GetInt32().Should().Be(400);
        document.RootElement.GetProperty("code").GetString().Should().Be("E42");
        body.Should().NotContain("lost");
    }

    private static void AssertStandardMembersOnly(string body)
    {
        using var document = JsonDocument.Parse(body);
        document.RootElement.GetProperty("status").GetInt32().Should().Be(400);
        document.RootElement.EnumerateObject().Select(p => p.Name).Should()
            .BeEquivalentTo(["type", "title", "status", "detail", "instance", "traceId"]);
    }

    private sealed class Node
    {
        public string Name { get; set; } = string.Empty;

        public Node? Next { get; set; }
    }

    private sealed class RecordingLogger<T>(List<(LogLevel Level, string Message)> log) : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => log.Add((logLevel, formatter(state, exception)));
    }

    private static async Task<string> RunAsync(Exception exception, List<(LogLevel Level, string Message)>? log = null)
    {
        ILogger<GlobalExceptionHandler> globalLogger = log is null
            ? NullLogger<GlobalExceptionHandler>.Instance
            : new RecordingLogger<GlobalExceptionHandler>(log);
        var factory = new ExceptionHandlerFactory(
            new ValidationExceptionHandler(NullLogger<ValidationExceptionHandler>.Instance),
            new GlobalExceptionHandler(globalLogger));
        var middleware = new ExceptionMiddleware(_ => throw exception, factory, NullLogger<ExceptionMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/test";

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}
