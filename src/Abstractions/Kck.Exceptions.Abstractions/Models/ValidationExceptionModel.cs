namespace Kck.Exceptions.Models;

/// <summary>
/// Describes validation errors for a single property. Used by <c>ValidationException</c>
/// to convey field-level details in the error response.
/// </summary>
public sealed class ValidationExceptionModel
{
    /// <summary>The name of the property that failed validation.</summary>
    public string Property { get; init; } = string.Empty;

    /// <summary>One or more human-readable error messages for <see cref="Property"/>.</summary>
    public IEnumerable<string> Errors { get; init; } = [];
}
