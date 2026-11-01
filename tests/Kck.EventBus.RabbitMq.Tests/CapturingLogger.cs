using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Kck.EventBus.RabbitMq.Tests;

/// <summary>One captured log entry: level, event name, rendered message and the structured values.</summary>
internal sealed record CapturedLogEntry(
    LogLevel Level,
    string? EventName,
    string Message,
    IReadOnlyList<KeyValuePair<string, object?>> Values);

/// <summary>Thread-safe <see cref="ILogger{TCategoryName}"/> that keeps every entry for assertions.</summary>
internal sealed class CapturingLogger<T> : ILogger<T>
{
    private readonly ConcurrentQueue<CapturedLogEntry> _entries = new();

    public IReadOnlyList<CapturedLogEntry> Entries => [.. _entries];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var values = state as IReadOnlyList<KeyValuePair<string, object?>> ?? [];
        _entries.Enqueue(new CapturedLogEntry(logLevel, eventId.Name, formatter(state, exception), [.. values]));
    }
}
