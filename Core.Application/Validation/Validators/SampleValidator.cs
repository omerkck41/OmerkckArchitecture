namespace Core.Application.Validation.Validators;

using FluentValidation;

public class SampleValidator : AbstractValidator<SampleRequest>
{
    public SampleValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Age).GreaterThan(18).WithMessage("Age must be greater than 18.");
    }
}

public class SampleRequest
{
    public string? Name { get; set; }
    public int Age { get; set; }
}