namespace Core.CrossCuttingConcerns.GlobalException.Exceptions;

public class ValidationException : CustomException
{
    public IEnumerable<ValidationExceptionModel> Errors { get; }

    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = Array.Empty<ValidationExceptionModel>();
    }

    public ValidationException(string? message)
        : base(message ?? "One or more validation failures have occurred.")
    {
        Errors = Array.Empty<ValidationExceptionModel>();
    }

    public ValidationException(string? message, Exception? innerException)
        : base(message ?? "One or more validation failures have occurred.",
               innerException ?? new Exception("No inner exception provided."))
    {
        Errors = Array.Empty<ValidationExceptionModel>();
    }

    public ValidationException(IEnumerable<ValidationExceptionModel> errors)
        : base(BuildErrorMessage(errors))
    {
        Errors = errors;
    }

    private static string BuildErrorMessage(IEnumerable<ValidationExceptionModel> errors)
    {
        IEnumerable<string> arr = errors.Select(x =>
            $"{Environment.NewLine} -- {x.Property}: {string.Join(Environment.NewLine, x.Errors ?? Array.Empty<string>())}"
        );
        return $"Validation failed: {string.Join(string.Empty, arr)}";
    }

}
public class ValidationExceptionModel
{
    public string? Property { get; set; }
    public IEnumerable<string>? Errors { get; set; }
    public string? ErrorCode { get; set; }
}