namespace Kck.AspNetCore.Sanitization;

public interface IInputSanitizer
{
    string Sanitize(string input);
    bool IsAlphanumeric(string input);
}
