using AwesomeAssertions;
using Kck.AspNetCore.Sanitization;
using Kck.AspNetCore.Session;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Kck.AspNetCore.Tests;

public sealed class BuilderAndPipelineTests
{
    private sealed class ListLoggerProvider : ILoggerProvider
    {
        public List<(LogLevel Level, string Category, string Message)> Entries { get; } = [];

        public ILogger CreateLogger(string categoryName) => new ListLogger(this, categoryName);

        public void Dispose()
        {
        }

        private sealed class ListLogger(ListLoggerProvider owner, string category) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter) =>
                owner.Entries.Add((logLevel, category, formatter(state, exception)));
        }
    }

    private static ServiceProvider BuildProvider(
        Action<KckAspNetCoreBuilder> configure,
        string environment = "Development",
        ListLoggerProvider? logs = null)
    {
        var services = new ServiceCollection();
        var env = Substitute.For<IWebHostEnvironment>();
        env.EnvironmentName.Returns(environment);
        services.AddSingleton<IHostEnvironment>(env);
        services.AddSingleton<IWebHostEnvironment>(env);
        services.AddLogging(l =>
        {
            if (logs is not null) l.AddProvider(logs);
        });
        services.AddKckAspNetCore(configure);
        return services.BuildServiceProvider();
    }

    // ---- DI registration ----

    [Fact]
    public void AddKckAspNetCore_RegistersBuilderSingleton()
    {
        using var provider = BuildProvider(b => b.UseSecurityHeaders());

        provider.GetRequiredService<KckAspNetCoreBuilder>().UseSecurityHeadersFlag.Should().BeTrue();
    }

    [Fact]
    public void AddInputSanitizer_RegistersSingletonSanitizer()
    {
        using var provider = BuildProvider(b => b.AddInputSanitizer());

        var first = provider.GetRequiredService<IInputSanitizer>();

        first.Should().BeOfType<InputSanitizer>();
        provider.GetRequiredService<IInputSanitizer>().Should().BeSameAs(first);
    }

    [Fact]
    public void AddCookieManager_RegistersSingletonCookieManager()
    {
        using var provider = BuildProvider(b => b.AddCookieManager());

        provider.GetRequiredService<ICookieManager>().Should().BeOfType<CookieManager>();
    }

    [Fact]
    public void AddSessionManager_RegistersSingletonSessionManager()
    {
        using var provider = BuildProvider(b => b.AddSessionManager());

        provider.GetRequiredService<ISessionManager>().Should().BeOfType<SessionManager>();
    }

    [Fact]
    public void AddInputSanitizer_PreRegisteredImplementation_IsNotReplaced()
    {
        var services = new ServiceCollection();
        var custom = Substitute.For<IInputSanitizer>();
        services.AddSingleton(custom);

        services.AddKckAspNetCore(b => b.AddInputSanitizer());

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IInputSanitizer>().Should().BeSameAs(custom);
    }

    [Fact]
    public void Builder_NothingConfigured_HasDefaultFlags()
    {
        using var provider = BuildProvider(_ => { });

        var builder = provider.GetRequiredService<KckAspNetCoreBuilder>();

        builder.UseSecurityHeadersFlag.Should().BeFalse();
        builder.UseRateLimitingFlag.Should().BeFalse();
        builder.CorsOrigins.Should().BeNull();
        builder.CorsPolicyName.Should().Be("KckCorsPolicy");
    }

    [Fact]
    public void UseRateLimiting_SetsFlagAndRegistersRateLimiterOptions()
    {
        using var provider = BuildProvider(b => b.UseRateLimiting(o => o.PermitLimit = 1));

        provider.GetRequiredService<KckAspNetCoreBuilder>().UseRateLimitingFlag.Should().BeTrue();
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>>().Value;
        options.RejectionStatusCode.Should().Be(429);
        options.GlobalLimiter.Should().NotBeNull();
    }

    [Fact]
    public void UseRateLimiting_GlobalLimiter_RejectsAfterPermitLimitPerClientIp()
    {
        using var provider = BuildProvider(b => b.UseRateLimiting(o => o.PermitLimit = 2));
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>>().Value;
        var limiter = options.GlobalLimiter!;

        var a = new DefaultHttpContext();
        a.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("1.1.1.1");
        var b = new DefaultHttpContext();
        b.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("2.2.2.2");

        using var l1 = limiter.AttemptAcquire(a);
        using var l2 = limiter.AttemptAcquire(a);
        using var l3 = limiter.AttemptAcquire(a);
        using var other = limiter.AttemptAcquire(b);

        l1.IsAcquired.Should().BeTrue();
        l2.IsAcquired.Should().BeTrue();
        l3.IsAcquired.Should().BeFalse();
        other.IsAcquired.Should().BeTrue();
    }

    [Fact]
    public void UseRateLimiting_NoRemoteIp_SharesUnknownPartition()
    {
        using var provider = BuildProvider(b => b.UseRateLimiting(o => o.PermitLimit = 1));
        var limiter = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>>().Value.GlobalLimiter!;

        using var first = limiter.AttemptAcquire(new DefaultHttpContext());
        using var second = limiter.AttemptAcquire(new DefaultHttpContext());

        first.IsAcquired.Should().BeTrue();
        second.IsAcquired.Should().BeFalse();
    }

    [Fact]
    public void KckRateLimitOptions_Defaults_Are100Per60Seconds()
    {
        var options = new KckRateLimitOptions();

        options.PermitLimit.Should().Be(100);
        options.WindowInSeconds.Should().Be(60);
    }

    [Fact]
    public void UseCorsPolicy_NullOrigins_StoresEmptyArrayAndDefaultName()
    {
        using var provider = BuildProvider(b => b.UseCorsPolicy());

        var builder = provider.GetRequiredService<KckAspNetCoreBuilder>();
        builder.CorsOrigins.Should().NotBeNull().And.BeEmpty();
        builder.CorsPolicyName.Should().Be("KckCorsPolicy");
    }

    [Fact]
    public async Task UseCorsPolicy_WithOrigins_RegistersRestrictedPolicy()
    {
        using var provider = BuildProvider(b => b.UseCorsPolicy(["https://a.example"], "MyPolicy"));
        var cors = provider.GetRequiredService<Microsoft.AspNetCore.Cors.Infrastructure.ICorsPolicyProvider>();

        var policy = await cors.GetPolicyAsync(new DefaultHttpContext(), "MyPolicy");

        policy.Should().NotBeNull();
        policy!.AllowAnyOrigin.Should().BeFalse();
        policy.Origins.Should().ContainSingle().Which.Should().Be("https://a.example");
        policy.AllowAnyMethod.Should().BeTrue();
        policy.AllowAnyHeader.Should().BeTrue();
    }

    [Fact]
    public async Task UseCorsPolicy_NoOrigins_RegistersAllowAnyOriginPolicy()
    {
        using var provider = BuildProvider(b => b.UseCorsPolicy());
        var cors = provider.GetRequiredService<Microsoft.AspNetCore.Cors.Infrastructure.ICorsPolicyProvider>();

        var policy = await cors.GetPolicyAsync(new DefaultHttpContext(), "KckCorsPolicy");

        policy!.AllowAnyOrigin.Should().BeTrue();
    }

    // ---- UseKckAspNetCore ----

    [Fact]
    public async Task UseKckAspNetCore_SecurityHeadersEnabled_PipelineSetsHeaders()
    {
        using var provider = BuildProvider(b => b.UseSecurityHeaders());
        var app = new ApplicationBuilder(provider);

        app.UseKckAspNetCore();
        var pipeline = app.Build();
        var context = new DefaultHttpContext { RequestServices = provider };
        await pipeline(context);

        context.Response.Headers.XContentTypeOptions.ToString().Should().Be("nosniff");
    }

    [Fact]
    public async Task UseKckAspNetCore_SecurityHeadersDisabled_PipelineAddsNoHeaders()
    {
        using var provider = BuildProvider(_ => { });
        var app = new ApplicationBuilder(provider);

        app.UseKckAspNetCore();
        var context = new DefaultHttpContext { RequestServices = provider };
        await app.Build()(context);

        context.Response.Headers.ContainsKey("X-Content-Type-Options").Should().BeFalse();
    }

    [Fact]
    public async Task UseKckAspNetCore_RateLimitingEnabled_SecondRequestFromSameIpGets429()
    {
        using var provider = BuildProvider(b => b.UseRateLimiting(o => o.PermitLimit = 1));
        var app = new ApplicationBuilder(provider);
        app.UseKckAspNetCore();
        var pipeline = app.Build();

        var first = new DefaultHttpContext { RequestServices = provider };
        var second = new DefaultHttpContext { RequestServices = provider };
        await pipeline(first);
        await pipeline(second);

        first.Response.StatusCode.Should().Be(404);
        second.Response.StatusCode.Should().Be(429);
    }

    [Fact]
    public async Task UseKckAspNetCore_CorsEnabled_PipelineBuildsAndPolicyAllowsOnlyConfiguredOrigin()
    {
        using var provider = BuildProvider(b => b.UseCorsPolicy(["https://a.example"]));
        var app = new ApplicationBuilder(provider);
        app.UseKckAspNetCore();
        var pipeline = app.Build();
        var policy = await provider.GetRequiredService<Microsoft.AspNetCore.Cors.Infrastructure.ICorsPolicyProvider>()
            .GetPolicyAsync(new DefaultHttpContext(), "KckCorsPolicy");
        var service = provider.GetRequiredService<Microsoft.AspNetCore.Cors.Infrastructure.ICorsService>();

        var allowed = new DefaultHttpContext { RequestServices = provider };
        allowed.Request.Headers.Origin = "https://a.example";
        var denied = new DefaultHttpContext { RequestServices = provider };
        denied.Request.Headers.Origin = "https://evil.example";
        await pipeline(allowed);

        service.EvaluatePolicy(allowed, policy!).IsOriginAllowed.Should().BeTrue();
        service.EvaluatePolicy(denied, policy!).IsOriginAllowed.Should().BeFalse();
    }

    [Fact]
    public void UseKckAspNetCore_AllowAnyOriginInProduction_LogsWarning()
    {
        var logs = new ListLoggerProvider();
        using var provider = BuildProvider(b => b.UseCorsPolicy(), "Production", logs);

        new ApplicationBuilder(provider).UseKckAspNetCore();

        logs.Entries.Should().ContainSingle(e => e.Level == LogLevel.Warning && e.Category == "Kck.AspNetCore.Cors")
            .Which.Message.Should().Contain("AllowAnyOrigin");
    }

    [Fact]
    public void UseKckAspNetCore_AllowAnyOriginInDevelopment_DoesNotLog()
    {
        var logs = new ListLoggerProvider();
        using var provider = BuildProvider(b => b.UseCorsPolicy(), "Development", logs);

        new ApplicationBuilder(provider).UseKckAspNetCore();

        logs.Entries.Should().BeEmpty();
    }

    [Fact]
    public void UseKckAspNetCore_ExplicitOriginsInProduction_DoesNotLog()
    {
        var logs = new ListLoggerProvider();
        using var provider = BuildProvider(b => b.UseCorsPolicy(["https://a.example"]), "Production", logs);

        new ApplicationBuilder(provider).UseKckAspNetCore();

        logs.Entries.Should().BeEmpty();
    }

    [Fact]
    public void UseKckAspNetCore_WithoutRegistration_Throws()
    {
        var provider = new ServiceCollection().BuildServiceProvider();

        var act = () => new ApplicationBuilder(provider).UseKckAspNetCore();

        act.Should().Throw<InvalidOperationException>();
    }
}
