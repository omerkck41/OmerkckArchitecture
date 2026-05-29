using FluentAssertions;
using Kck.Pipeline.Abstractions;
using Kck.Pipeline.Mediator.Behaviors;
using Mediator;
using Xunit;

namespace Kck.Pipeline.Mediator.Tests;

public sealed class LoggingBehaviorTests
{
    private sealed record LogMessage : IRequest<string>, ILoggableRequest;

    [Fact]
    public async Task Handle_FastRequest_LogsHandledNotLongRunning()
    {
        var logger = new CapturingLogger<LoggingBehavior<LogMessage, string>>();
        var sut = new LoggingBehavior<LogMessage, string>(logger);

        var result = await sut.Handle(new LogMessage(), (_, _) => ValueTask.FromResult("ok"), CancellationToken.None);

        result.Should().Be("ok");
        logger.Entries.Should().Contain(e => e.Contains("Handling"));
        logger.Entries.Should().Contain(e => e.StartsWith("Handled "));
        logger.Entries.Should().NotContain(e => e.Contains("Long running"));
    }

    [Fact]
    public async Task Handle_SlowRequest_LogsLongRunningWarning()
    {
        var logger = new CapturingLogger<LoggingBehavior<LogMessage, string>>();
        var sut = new LoggingBehavior<LogMessage, string>(logger);

        var result = await sut.Handle(
            new LogMessage(),
            async (_, ct) =>
            {
                await Task.Delay(600, ct);
                return "ok";
            },
            CancellationToken.None);

        result.Should().Be("ok");
        logger.Entries.Should().Contain(e => e.Contains("Long running"));
        logger.Entries.Should().NotContain(e => e.StartsWith("Handled "));
    }
}
