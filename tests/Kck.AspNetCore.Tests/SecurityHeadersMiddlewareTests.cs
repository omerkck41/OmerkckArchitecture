using FluentAssertions;
using Kck.AspNetCore.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace Kck.AspNetCore.Tests;

public sealed class SecurityHeadersMiddlewareTests
{
    private static IWebHostEnvironment CreateEnv(string environmentName)
    {
        var env = Substitute.For<IWebHostEnvironment>();
        env.EnvironmentName.Returns(environmentName);
        return env;
    }

    [Fact]
    public async Task InvokeAsync_SetsBaseSecurityHeaders()
    {
        var context = new DefaultHttpContext();
        var sut = new SecurityHeadersMiddleware(_ => Task.CompletedTask, CreateEnv("Development"));

        await sut.InvokeAsync(context);

        context.Response.Headers.XFrameOptions.ToString().Should().Be("DENY");
        context.Response.Headers.XXSSProtection.ToString().Should().Be("0");
        context.Response.Headers.XContentTypeOptions.ToString().Should().Be("nosniff");
        context.Response.Headers["Referrer-Policy"].ToString().Should().Be("strict-origin-when-cross-origin");
        context.Response.Headers["Permissions-Policy"].ToString().Should().Be("camera=(), microphone=(), geolocation=()");
        context.Response.Headers.ContentSecurityPolicy.ToString().Should().Be("default-src 'self'");
    }

    [Fact]
    public async Task InvokeAsync_HttpsAndProduction_AddsHsts()
    {
        var context = new DefaultHttpContext();
        context.Request.IsHttps = true;
        var sut = new SecurityHeadersMiddleware(_ => Task.CompletedTask, CreateEnv("Production"));

        await sut.InvokeAsync(context);

        context.Response.Headers.StrictTransportSecurity.ToString()
            .Should().Be("max-age=31536000; includeSubDomains; preload");
    }

    [Fact]
    public async Task InvokeAsync_HttpsButDevelopment_DoesNotAddHsts()
    {
        var context = new DefaultHttpContext();
        context.Request.IsHttps = true;
        var sut = new SecurityHeadersMiddleware(_ => Task.CompletedTask, CreateEnv("Development"));

        await sut.InvokeAsync(context);

        context.Response.Headers.StrictTransportSecurity.ToString().Should().BeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_HttpAndProduction_DoesNotAddHsts()
    {
        var context = new DefaultHttpContext();
        var sut = new SecurityHeadersMiddleware(_ => Task.CompletedTask, CreateEnv("Production"));

        await sut.InvokeAsync(context);

        context.Response.Headers.StrictTransportSecurity.ToString().Should().BeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_CallsNextDelegate()
    {
        var nextCalled = false;
        var sut = new SecurityHeadersMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            CreateEnv("Development"));

        await sut.InvokeAsync(new DefaultHttpContext());

        nextCalled.Should().BeTrue();
    }
}
