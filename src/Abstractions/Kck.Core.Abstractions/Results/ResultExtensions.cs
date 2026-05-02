namespace Kck.Core.Abstractions.Results;

/// <summary>
/// Functional pipeline extensions for <see cref="Result{T}"/>:
/// Map (transform value), Bind (chain operations), Tap (side effects), Ensure (guard).
/// </summary>
public static class ResultExtensions
{
    /// <summary>Transforms the success value; propagates failure unchanged.</summary>
    public static Result<TOut> Map<T, TOut>(this Result<T> result, Func<T, TOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        return result.IsSuccess
            ? Result<TOut>.Success(mapper(result.Value!))
            : Result<TOut>.Failure(result.Error!);
    }

    /// <summary>Chains a Result-returning operation; propagates failure without calling binder.</summary>
    public static Result<TOut> Bind<T, TOut>(this Result<T> result, Func<T, Result<TOut>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        return result.IsSuccess
            ? binder(result.Value!)
            : Result<TOut>.Failure(result.Error!);
    }

    /// <summary>Executes a side-effect action on success; returns the original result unchanged.</summary>
    public static Result<T> Tap<T>(this Result<T> result, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (result.IsSuccess)
            action(result.Value!);
        return result;
    }

    /// <summary>Converts to failure if the predicate is not satisfied; propagates existing failure.</summary>
    public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, Error error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        if (!result.IsSuccess)
            return result;
        return predicate(result.Value!)
            ? result
            : Result<T>.Failure(error);
    }
}
