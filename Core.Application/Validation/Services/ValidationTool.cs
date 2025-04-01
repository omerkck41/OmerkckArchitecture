using Core.CrossCuttingConcerns.GlobalException.Models;
using FluentValidation;
using FluentValidation.Results;
using ValidationException = Core.CrossCuttingConcerns.GlobalException.Exceptions.ValidationException;

namespace Core.Application.Validation.Services;

public class ValidationTool
{
    public static void Validate(IValidator validator, object entity)
    {
        ValidationContext<object> context = new(entity);
        ValidationResult result = validator.Validate(context);

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors.Select(e => new ValidationExceptionModel
            {
                Property = e.PropertyName,
                Errors = [e.ErrorMessage]
            }));
        }
    }

    public static async Task ValidateAsync(IValidator validator, object entity, CancellationToken cancellationToken = default)
    {
        ValidationContext<object> context = new(entity);
        ValidationResult result = await validator.ValidateAsync(context, cancellationToken);

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors.Select(e => new ValidationExceptionModel
            {
                Property = e.PropertyName,
                Errors = new List<string> { e.ErrorMessage }
            }));
        }
    }
}
