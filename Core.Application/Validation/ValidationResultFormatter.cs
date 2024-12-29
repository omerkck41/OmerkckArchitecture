namespace Core.Application.Validation;

using FluentValidation.Results;

public static class ValidationResultFormatter
{
    public static string Format(IEnumerable<ValidationFailure> failures)
    {
        return string.Join("; ", failures.Select(f => $"Property: {f.PropertyName} failed validation. Error: {f.ErrorMessage}"));
    }
}