using System.Diagnostics;

namespace Kck.Core.Abstractions.Results;

/// <summary>
/// Represents the outcome of a void operation (no return value).
/// </summary>
[DebuggerDisplay("Success: {IsSuccess}, Error: {Error?.Message,nq}")]
public sealed class Result
{
    /// <summary><see langword="true"/> when the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Failure details. <see langword="null"/> when <see cref="IsSuccess"/> is <see langword="true"/>.</summary>
    public Error? Error { get; }

    private Result() { IsSuccess = true; }
    private Result(Error error) { IsSuccess = false; Error = error; }

    /// <summary>Creates a successful result.</summary>
    public static Result Success() => new();

    /// <summary>Creates a failure result with the specified error.</summary>
    /// <param name="error">The error that caused the failure.</param>
    public static Result Failure(Error error) => new(error);

    /// <summary>Executes <paramref name="onSuccess"/> or <paramref name="onFailure"/> depending on the result state.</summary>
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess() : onFailure(Error!);
}

/// <summary>
/// Represents the outcome of an operation that returns a value of type <typeparamref name="T"/>.
/// </summary>
[DebuggerDisplay("Success: {IsSuccess}, Value: {Value}, Error: {Error?.Message,nq}")]
public sealed class Result<T>
{
    /// <summary><see langword="true"/> when the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>The operation result value. Non-null only when <see cref="IsSuccess"/> is <see langword="true"/>.</summary>
    public T? Value { get; }

    /// <summary>Failure details. <see langword="null"/> when <see cref="IsSuccess"/> is <see langword="true"/>.</summary>
    public Error? Error { get; }

    private Result(T value) { IsSuccess = true; Value = value; }
    private Result(Error error) { IsSuccess = false; Error = error; }

#pragma warning disable CA1000
    /// <summary>Creates a successful result with the specified value.</summary>
    /// <param name="value">The operation's return value.</param>
    public static Result<T> Success(T value) => new(value);

    /// <summary>Creates a failure result with the specified error.</summary>
    /// <param name="error">The error that caused the failure.</param>
    public static Result<T> Failure(Error error) => new(error);
#pragma warning restore CA1000

    /// <summary>Executes <paramref name="onSuccess"/> or <paramref name="onFailure"/> depending on the result state.</summary>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(Error!);
}
