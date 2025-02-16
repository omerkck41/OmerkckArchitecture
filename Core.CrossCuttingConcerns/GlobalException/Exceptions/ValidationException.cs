using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class ValidationException : CustomException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    public ProblemDetails ToProblemDetails()
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation error",
            Detail = "Validation failed for one or more fields.",
            Extensions = { ["errors"] = Errors }
        };
    }
}