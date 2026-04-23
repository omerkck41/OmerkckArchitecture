using System.Text.Json.Serialization;

namespace Kck.Exceptions.AspNetCore.Models;

/// <summary>
/// RFC 7807 Problem Details standardına uyumlu hata yanıtı.
/// </summary>
public sealed record UnifiedApiErrorResponse
{
    /// <summary>
    /// RFC 7807 — hata tipi URI'si.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// RFC 7807 — hata başlığı.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// HTTP durum kodu.
    /// </summary>
    public int Status { get; init; }

    /// <summary>
    /// Hata detay mesajı.
    /// </summary>
    public string Detail { get; init; }

    /// <summary>
    /// İstek yolu (request path).
    /// </summary>
    public string? Instance { get; init; }

    /// <summary>
    /// Validasyon hataları (opsiyonel).
    /// Key: alan adı, Value: hata mesajları listesi.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, string[]>? Errors { get; init; }

    /// <summary>
    /// Korelasyon/takip kimliği.
    /// </summary>
    public string TraceId { get; init; }

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
