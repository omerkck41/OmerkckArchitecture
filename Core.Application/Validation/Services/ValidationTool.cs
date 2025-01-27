using FluentValidation;
using FluentValidation.Results;

namespace Core.Application.Validation.Services;

public class ValidationTool
{
    public static void Validate(IValidator validator, object entity)
    {
        ValidationContext<object> context = new(entity);
        ValidationResult result = validator.Validate(context);

        if (!result.IsValid)
        {
            throw new ValidationException(ValidationResultFormatter.Format(result.Errors));
        }
    }

    public static async Task ValidateAsync(IValidator validator, object entity, CancellationToken cancellationToken = default)
    {
        ValidationContext<object> context = new(entity);
        ValidationResult result = await validator.ValidateAsync(context, cancellationToken);

        if (!result.IsValid)
        {
            throw new ValidationException(ValidationResultFormatter.Format(result.Errors));
        }
    }
}