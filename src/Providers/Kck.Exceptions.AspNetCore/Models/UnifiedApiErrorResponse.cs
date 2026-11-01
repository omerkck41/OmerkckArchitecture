using System.Text.Json.Serialization;

namespace Kck.Exceptions.AspNetCore.Models;

/// <summary>
/// Error response compliant with the RFC 7807 Problem Details standard.
/// </summary>
public sealed record UnifiedApiErrorResponse
{
    /// <summary>
    /// RFC 7807 — error type URI.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// RFC 7807 — error title.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// HTTP status code.
    /// </summary>
    public int Status { get; init; }

    /// <summary>
    /// Error detail message.
    /// </summary>
    public string Detail { get; init; }

    /// <summary>
    /// Request path.
    /// </summary>
    public string? Instance { get; init; }

    /// <summary>
    /// Validation errors (optional).
    /// Key: field name, Value: list of error messages.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, string[]>? Errors { get; init; }

    /// <summary>
    /// Correlation/tracking identifier.
    /// </summary>
    public string TraceId { get; init; }

    /// <summary>
    /// Extension members (RFC 9457 §3.2), filled from <c>CustomException.AdditionalData</c>.
    /// The pairs are written flat into the response body next to the standard members; <c>null</c> writes nothing.
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, object?>? Extensions { get; init; }

    [JsonConstructor]
    public UnifiedApiErrorResponse(
        string? type,
        string title,
        int status,
        string detail,
        string? instance,
        IDictionary<string, string[]>? errors,
        string traceId)
    {
        Type = type;
        Title = title;
        Status = status;
        Detail = detail;
        Instance = instance;
        Errors = errors;
        TraceId = traceId;
    }
}
