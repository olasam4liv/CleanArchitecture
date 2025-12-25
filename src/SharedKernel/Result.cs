using System.Diagnostics.CodeAnalysis;

namespace SharedKernel;

public class Result
{
    public Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Failure<TValue>(Error error) =>
        new(default, false, error);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    public Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can't be accessed.");

    /// <remarks>
    /// Implicitly converts a nullable value to a Result{TValue}.
    /// Succeeds if value is not null; fails with NullValue error if null.
    /// Use ToResult() for explicit conversion.
    /// </remarks>
#pragma warning disable CA2225 // Provide named method for operator - Implicit conversion is intentional
    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
#pragma warning restore CA2225

    /// <summary>
    /// Explicitly converts a Result{TValue} to its underlying value.
    /// Throws if result is failure. Use FromResult() for explicit conversion.
    /// </summary>
#pragma warning disable CA2225 // Provide named method for operator - Explicit conversion is intentional
#pragma warning disable CA1062 // Validate parameter 'result' - null check in Value getter
    public static explicit operator TValue(Result<TValue> result) =>
        result.Value;
#pragma warning restore CA1062
#pragma warning restore CA2225

    public static TValue? ToValue(Result<TValue> result) => result.IsSuccess ? result.Value : default;

#pragma warning disable CA1000 // Do not declare static on generic - Factory pattern is intentional
    public static Result<TValue> ValidationFailure(Error error) =>
        new(default, false, error);
#pragma warning restore CA1000
}
