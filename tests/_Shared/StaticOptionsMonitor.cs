using Microsoft.Extensions.Options;

namespace Kck.Testing;

/// <summary>
/// Test double for <see cref="IOptionsMonitor{TOptions}"/> that returns a fixed value
/// and ignores change subscriptions. Use in place of <c>Options.Create</c>.
/// </summary>
internal sealed class StaticOptionsMonitor<T> : IOptionsMonitor<T>
{
    public StaticOptionsMonitor(T value)
    {
        CurrentValue = value;
    }

    public T CurrentValue { get; }

    public T Get(string? name) => CurrentValue;

    public IDisposable? OnChange(Action<T, string?> listener) => null;
}
