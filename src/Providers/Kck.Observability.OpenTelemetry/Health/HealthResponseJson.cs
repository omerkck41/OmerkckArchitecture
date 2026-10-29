using System.Text.Json.Serialization;

namespace Kck.Observability.OpenTelemetry;

// JSON body of the MapKckHealthChecks endpoints: {"status","checks":[{"name","status","description","duration"}],
// "totalDuration"}; durations in milliseconds.
internal sealed record HealthReportBody(string Status, IReadOnlyList<HealthEntryBody> Checks, double TotalDuration);

internal sealed record HealthEntryBody(string Name, string Status, string? Description, double Duration);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(HealthReportBody))]
internal sealed partial class HealthJsonContext : JsonSerializerContext;
