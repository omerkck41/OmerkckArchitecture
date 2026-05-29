using AwesomeAssertions;
using Kck.Logging.Serilog;
using Serilog.Events;
using Serilog.Parsing;
using Xunit;

namespace Kck.Logging.Serilog.Tests;

public sealed class PiiMaskingEnricherTests
{
    private static readonly MessageTemplate EmptyTemplate =
        new MessageTemplateParser().Parse(string.Empty);

    private static LogEvent MakeEvent(params (string Name, object? Value)[] properties)
    {
        var props = properties.Select(p =>
            new LogEventProperty(p.Name, new ScalarValue(p.Value)));
        return new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            exception: null,
            EmptyTemplate,
            props);
    }

    private static string? StringValue(LogEvent e, string key) =>
        e.Properties.TryGetValue(key, out var v) && v is ScalarValue sv
            ? sv.Value?.ToString()
            : null;

    [Fact]
    public void Enrich_MaskedProperty_IsReplaced()
    {
        var sut = new PiiMaskingEnricher(["email"]);
        var evt = MakeEvent(("email", "user@example.com"), ("name", "Alice"));

        sut.Enrich(evt, new LogEventPropertyFactory());

        StringValue(evt, "email").Should().Be("***");
        StringValue(evt, "name").Should().Be("Alice");
    }

    [Fact]
    public void Enrich_IsCaseInsensitive()
    {
        var sut = new PiiMaskingEnricher(["password"]);
        var evt = MakeEvent(("Password", "secret123"));

        sut.Enrich(evt, new LogEventPropertyFactory());

        StringValue(evt, "Password").Should().Be("***");
    }

    [Fact]
    public void Enrich_NoMatchingProperty_LeavesEventUnchanged()
    {
        var sut = new PiiMaskingEnricher(["password"]);
        var evt = MakeEvent(("username", "alice"));

        sut.Enrich(evt, new LogEventPropertyFactory());

        StringValue(evt, "username").Should().Be("alice");
    }

    [Fact]
    public void Enrich_MultipleMatchingProperties_AllMasked()
    {
        var sut = new PiiMaskingEnricher(["email", "token"]);
        var evt = MakeEvent(("email", "a@b.com"), ("token", "abc123"), ("id", "42"));

        sut.Enrich(evt, new LogEventPropertyFactory());

        StringValue(evt, "email").Should().Be("***");
        StringValue(evt, "token").Should().Be("***");
        StringValue(evt, "id").Should().Be("42");
    }

    [Fact]
    public void DefaultPiiProperties_ContainsExpectedKeys()
    {
        PiiMaskingEnricher.DefaultPiiProperties.Should().Contain("password");
        PiiMaskingEnricher.DefaultPiiProperties.Should().Contain("token");
        PiiMaskingEnricher.DefaultPiiProperties.Should().Contain("email");
        PiiMaskingEnricher.DefaultPiiProperties.Should().Contain("creditcard");
    }
}

internal sealed class LogEventPropertyFactory : global::Serilog.Core.ILogEventPropertyFactory
{
    public LogEventProperty CreateProperty(string name, object? value, bool destructureObjects = false)
        => new(name, new ScalarValue(value));
}
