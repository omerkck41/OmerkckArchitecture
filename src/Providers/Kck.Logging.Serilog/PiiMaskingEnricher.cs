using Serilog.Core;
using Serilog.Events;

namespace Kck.Logging.Serilog;

/// <summary>
/// Serilog enricher that replaces the values of named log properties with
/// <c>***</c> to prevent PII from appearing in log sinks.
/// </summary>
/// <remarks>
/// Register via <c>KckSerilogBuilder.UsePiiMasking()</c> or
/// <c>KckSerilogBuilder.MaskPiiProperties("email", "token", ...)</c>.
/// Matching is case-insensitive.
/// </remarks>
public sealed class PiiMaskingEnricher : ILogEventEnricher
{
    private const string Mask = "***";

    private readonly HashSet<string> _maskedProperties;

    /// <summary>
    /// Commonly masked PII property names (case-insensitive).
    /// Used by <c>KckSerilogBuilder.UsePiiMasking()</c> when no custom list is provided.
    /// </summary>
    public static readonly IReadOnlyList<string> DefaultPiiProperties =
    [
        "password", "pwd", "secret",
        "token", "accesstoken", "refreshtoken", "apikey",
        "email",
        "phonenumber", "phone",
        "creditcard", "cardnumber", "cvv",
        "ssn", "socialsecuritynumber"
    ];

    /// <param name="propertiesToMask">Property names to redact. Matching is case-insensitive.</param>
    public PiiMaskingEnricher(IEnumerable<string> propertiesToMask)
    {
        _maskedProperties = new HashSet<string>(propertiesToMask, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var hits = logEvent.Properties.Keys
            .Where(k => _maskedProperties.Contains(k))
            .ToList();

        foreach (var key in hits)
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(key, Mask));
    }
}
