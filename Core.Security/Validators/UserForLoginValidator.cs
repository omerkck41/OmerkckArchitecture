using Core.Security.Dtos;
using FluentValidation;

namespace Core.Security.Validators;

public class UserForLoginValidator : AbstractValidator<UserForLoginDto>
{
    public UserForLoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.")
                             .EmailAddress().WithMessage("Invalid email format.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}