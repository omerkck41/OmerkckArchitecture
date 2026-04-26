using System.Text.Json;
using BenchmarkDotNet.Attributes;

namespace Kck.Benchmarks;

/// <summary>
/// Baseline measurement for reflection-based JSON (de)serialization. LS-FAZ-6
/// will introduce <c>JsonSerializerContext</c> source-gen variants and compare
/// against this baseline.
/// </summary>
[MemoryDiagnoser]
public class JsonSerializationBenchmarks
{
    private static readonly SamplePayload Payload = new(
        Id: Guid.NewGuid(),
        Name: "Kck Benchmark Sample",
        Tags: new[] { "alpha", "beta", "gamma" },
        Score: 99.5);

    private static readonly string SerializedPayload = JsonSerializer.Serialize(Payload);

    [Benchmark]
    public string Serialize_Reflection() => JsonSerializer.Serialize(Payload);

    [Benchmark]
    public SamplePayload? Deserialize_Reflection() =>
        JsonSerializer.Deserialize<SamplePayload>(SerializedPayload);
}

public sealed record SamplePayload(Guid Id, string Name, string[] Tags, double Score);
