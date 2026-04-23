using System.Text.Json;
using FluentAssertions;
using Kck.Exceptions;
using Kck.Exceptions.AspNetCore.Handlers;
using Kck.Exceptions.AspNetCore.Middleware;
using Kck.Exceptions.AspNetCore.Models;
using Kck.Exceptions.Models;
using Microsoft.AspNetCore.Http;
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
    public async Task InvokeAsync_OperationCanceledException_ShouldRethrow()
    {
        var middleware = CreateMiddleware(_ => throw new OperationCanceledException());
        var context = CreateHttpContext();

        var act = () => middleware.InvokeAsync(context);

        await act.Should().ThrowAsync<OperationCanceledException>();
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
