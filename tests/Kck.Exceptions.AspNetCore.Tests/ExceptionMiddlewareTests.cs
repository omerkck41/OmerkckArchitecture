using System.Text.Json;
using AwesomeAssertions;
using Kck.Exceptions;
using Kck.Exceptions.AspNetCore.Handlers;
using Kck.Exceptions.AspNetCore.Middleware;
using Kck.Exceptions.AspNetCore.Models;
using Kck.Exceptions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kck.Exceptions.AspNetCore.Tests;

public class ExceptionMiddlewareTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public async Task InvokeAsync_BadRequestException_ShouldReturn400()
    {
        var middleware = CreateMiddleware(_ => throw new BadRequestException("Invalid input"));
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(400);
        var body = await ReadResponseBody(context);
        body.Title.Should().Be("BadRequestException");
    }

    [Fact]
    public async Task InvokeAsync_NotFoundException_ShouldReturn404()
    {
        var middleware = CreateMiddleware(_ => throw new NotFoundException("Not found"));
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(404);
        var body = await ReadResponseBody(context);
        body.Detail.Should().Be("Not found");
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedException_ShouldReturn401()
    {
        var middleware = CreateMiddleware(_ => throw new UnauthorizedException());
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_ForbiddenException_ShouldReturn403()
    {
        var middleware = CreateMiddleware(_ => throw new ForbiddenException());
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_ShouldReturn422()
    {
        var errors = new[]
        {
            new ValidationExceptionModel { Property = "Name", Errors = ["Required"] }
        };
        var middleware = CreateMiddleware(_ => throw new ValidationException(errors));
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(422);
        var body = await ReadResponseBody(context);
        body.Title.Should().Be("Validation Error");
        body.Errors.Should().ContainKey("Name");
    }

    [Fact]
    public async Task InvokeAsync_OperationCanceledException_ClientAborted_ShouldRethrow()
    {
        using var aborted = new CancellationTokenSource();
        await aborted.CancelAsync();
        var middleware = CreateMiddleware(_ => throw new OperationCanceledException(aborted.Token));
        var context = CreateHttpContext();
        context.RequestAborted = aborted.Token;

        var act = () => middleware.InvokeAsync(context);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task InvokeAsync_OtherException_ClientAborted_IsHandledWithoutWriting()
    {
        // The client is gone: the error is still handled (and logged by the handler), but nothing is written, so the
        // original failure is not turned into an OperationCanceledException by the write.
        using var aborted = new CancellationTokenSource();
        await aborted.CancelAsync();
        var middleware = CreateMiddleware(_ => throw new InvalidOperationException("Database unavailable"));
        var context = CreateHttpContext();
        context.RequestAborted = aborted.Token;

        var act = () => middleware.InvokeAsync(context);

        await act.Should().NotThrowAsync();
        context.Response.Body.Length.Should().Be(0);
        context.Response.StatusCode.Should().Be(500); // metrics and request logs still see the failure
    }

    [Fact]
    public async Task InvokeAsync_TaskCanceledException_RequestNotAborted_ShouldReturn500()
    {
        // An HttpClient timeout: canceled, but not by the client of this request.
        var middleware = CreateMiddleware(_ => throw new TaskCanceledException("The request was canceled due to the configured HttpClient.Timeout"));
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(500);
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task InvokeAsync_NoException_ShouldPassThrough()
    {
        var middleware = CreateMiddleware(ctx =>
        {
            ctx.Response.StatusCode = 200;
            return Task.CompletedTask;
        });
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(200);
    }

    [Theory]
    [InlineData("not-found", LogLevel.Warning)]       // 404
    [InlineData("validation", LogLevel.Warning)]      // 422
    [InlineData("unclassified", LogLevel.Error)]      // CustomException without a status: 500
    [InlineData("other", LogLevel.Error)]             // not a CustomException: 500
    public async Task InvokeAsync_LogsEachExceptionOnce_WarningForClientErrorsErrorForServerErrors(string kind, LogLevel expected)
    {
        // Before 4.7 the middleware logged every exception at Error and the handler logged it again.
        Exception thrown = kind switch
        {
            "not-found" => new NotFoundException("missing"),
            "validation" => new ValidationException([new ValidationExceptionModel { Property = "Name", Errors = ["Required"] }]),
            "unclassified" => new UnclassifiedException(),
            _ => new InvalidOperationException("boom"),
        };
        var log = new List<LogLevel>();
        var middleware = CreateMiddleware(_ => throw thrown, log);

        await middleware.InvokeAsync(CreateHttpContext());

        log.Should().Equal(expected);
    }

    private sealed class UnclassifiedException() : CustomException("unclassified");

    private sealed class RecordingLogger<T>(List<LogLevel> log) : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => log.Add(logLevel);
    }

    private static ExceptionMiddleware CreateMiddleware(RequestDelegate next, List<LogLevel> log)
    {
        var factory = new ExceptionHandlerFactory(
            new ValidationExceptionHandler(new RecordingLogger<ValidationExceptionHandler>(log)),
            new GlobalExceptionHandler(new RecordingLogger<GlobalExceptionHandler>(log)));
        return new ExceptionMiddleware(next, factory, new RecordingLogger<ExceptionMiddleware>(log));
    }

    private static ExceptionMiddleware CreateMiddleware(RequestDelegate next)
    {
        var validationHandler = new ValidationExceptionHandler(
            new NullLogger<ValidationExceptionHandler>());
        var globalHandler = new GlobalExceptionHandler(
            new NullLogger<GlobalExceptionHandler>());
        var factory = new ExceptionHandlerFactory(validationHandler, globalHandler);
        return new ExceptionMiddleware(next, factory, new NullLogger<ExceptionMiddleware>());
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/test";
        return context;
    }

    private static async Task<UnifiedApiErrorResponse> ReadResponseBody(DefaultHttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        return (await JsonSerializer.DeserializeAsync<UnifiedApiErrorResponse>(
            context.Response.Body, JsonOptions))!;
    }
}
