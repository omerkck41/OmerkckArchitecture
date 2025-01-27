using FluentValidation;

namespace Core.Application.Validation.Validators;

public static class CommonRules
{
    public static IRuleBuilderOptions<T, string> MustBeValidEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }

    public static IRuleBuilderOptions<T, string> MustBeValidPhoneNumber<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[1-9][0-9]{7,14}$").WithMessage("Invalid phone number format.");
    }

    public static IRuleBuilderOptions<T, string> MustBeValidUrl<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("URL is required.")
            .Matches(@"^(https?:\\/\\/)?([\\w.-]+)+(:\\d+)?(\\/\\S*)?$").WithMessage("Invalid URL format.");
    }

    public static IRuleBuilderOptions<T, string> MustBeNumeric<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Field is required.")
            .Matches("^[0-9]+$").WithMessage("Field must be numeric.");
    }

    public static IRuleBuilderOptions<T, string> MustContainUppercase<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Field is required.")
            .Matches("[A-Z]").WithMessage("Field must contain at least one uppercase letter.");
    }

    public static IRuleBuilderOptions<T, string> MustNotExceedMaxLength<T>(this IRuleBuilder<T, string> ruleBuilder, int maxLength)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Field is required.")
            .MaximumLength(maxLength).WithMessage($"Maximum length is {maxLength} characters.");
    }

    public static IRuleBuilderOptions<T, int> MustBeGreaterThanZero<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0).WithMessage("Value must be greater than zero.");
    }

    public static IRuleBuilderOptions<T, DateTime> MustBeInThePast<T>(this IRuleBuilder<T, DateTime> ruleBuilder)
    {
        return ruleBuilder
            .LessThan(DateTime.Now).WithMessage("Date must be in the past.");
    }

    public static IRuleBuilderOptions<T, string> MustBeAlphaNumeric<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Field is required.")
            .Matches("^[a-zA-Z0-9]+$").WithMessage("Field must be alphanumeric.");
    }

    public static IRuleBuilderOptions<T, int> MustBeInRange<T>(this IRuleBuilder<T, int> ruleBuilder, int min, int max)
    {
        return ruleBuilder
            .InclusiveBetween(min, max).WithMessage($"Value must be between {min} and {max}.");
    }
    public static IRuleBuilderOptions<T, string> MustContainSpecialCharacter<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Field is required.")
            .Matches("[!@#$%^&*(),.?\":{}|<>]").WithMessage("Field must contain at least one special character.");
    }
    public static IRuleBuilderOptions<T, string> MustMatchPattern<T>(this IRuleBuilder<T, string> ruleBuilder, string pattern, string errorMessage)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Field is required.")
            .Matches(pattern).WithMessage(errorMessage);
    }
    public static IRuleBuilderOptions<T, DateTime> MustBeWithinRange<T>(this IRuleBuilder<T, DateTime> ruleBuilder, DateTime min, DateTime max)
    {
        return ruleBuilder
            .InclusiveBetween(min, max).WithMessage($"Date must be between {min.ToShortDateString()} and {max.ToShortDateString()}.");
    }

}