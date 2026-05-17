namespace Kck.AspNetCore.Sanitization;

/// <summary>
/// Defines operations for sanitizing and validating user-supplied string input
/// before it is processed or persisted.
/// </summary>
public interface IInputSanitizer
{
    /// <summary>
    /// Removes potentially dangerous characters from <paramref name="input"/> and returns the
    /// cleaned string.
    /// </summary>
    string Sanitize(string input);

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="input"/> contains only ASCII letters
    /// and digits; otherwise <see langword="false"/>.
    /// </summary>
    bool IsAlphanumeric(string input);
}
