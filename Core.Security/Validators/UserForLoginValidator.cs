using Core.Security.Dtos;
using FluentValidation;

namespace Core.Security.Validators;

public class UserForLoginValidator : AbstractValidator<UserForLoginDto>
{
    public UserForLoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .WithErrorCode("EMAIL_REQUIRED")
            .EmailAddress().WithMessage("Invalid email format.")
            .WithErrorCode("INVALID_EMAIL_FORMAT");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .WithErrorCode("PASSWORD_REQUIRED");
    }
}