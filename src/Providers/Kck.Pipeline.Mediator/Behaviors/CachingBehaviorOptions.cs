using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Kck.Pipeline.Mediator.Behaviors;

/// <summary>
/// Options for <see cref="CachingBehavior{TMessage, TResponse}"/>.
/// </summary>
public sealed class CachingBehaviorOptions
{
    // The trimmer treats IsReflectionEnabledByDefault as a constant: it is false under Native AOT and in trimmed apps
    // that disable reflection, so JsonSerializerOptions.Default is never reached there.
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Guarded by JsonSerializer.IsReflectionEnabledByDefault.")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Guarded by JsonSerializer.IsReflectionEnabledByDefault.")]
    internal static JsonSerializerOptions DefaultSerializerOptions() =>
        JsonSerializer.IsReflectionEnabledByDefault
            ? JsonSerializerOptions.Default
            : throw new InvalidOperationException(
                "Reflection-based JSON serialization is disabled (Native AOT or trimming). Set " +
                "CachingBehaviorOptions.SerializerOptions through UseCachingBehavior(o => ...) to options whose " +
                "TypeInfoResolver is a source-generated JsonSerializerContext covering the cached response types.");

    /// <summary>
    /// Serializer options for cached responses. Native AOT apps set options whose <c>TypeInfoResolver</c> is a
    /// source-generated <c>JsonSerializerContext</c> covering every cached response type. <c>null</c> uses the
    /// default settings with reflection-based metadata.
    /// </summary>
    public JsonSerializerOptions? SerializerOptions { get; set; }
}
