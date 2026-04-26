using System.Diagnostics;

namespace Kck.Core.Abstractions.Results;

/// <summary>
/// Represents the outcome of a void operation (no return value).
/// </summary>
[DebuggerDisplay("Success: {IsSuccess}, Error: {Error?.Message,nq}")]
public sealed class Result
{
    public bool IsSuccess { get; }
    public Error? Error { get; }

    private Result() { IsSuccess = true; }
    private Result(Error error) { IsSuccess = false; Error = error; }

    public static Result Success() => new();
    public static Result Failure(Error error) => new(error);

    public TResult Match<TResult>(Func<TResult> onSuccess, Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess() : onFailure(Error!);
}

/// <summary>
/// Represents the outcome of an operation that returns a value of type <typeparamref name="T"/>.
/// </summary>
[DebuggerDisplay("Success: {IsSuccess}, Value: {Value}, Error: {Error?.Message,nq}")]
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error? Error { get; }

    private Result(T value) { IsSuccess = true; Value = value; }
    private Result(Error error) { IsSuccess = false; Error = error; }

#pragma warning disable CA1000
    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);
#pragma warning restore CA1000

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(Error!);
}
