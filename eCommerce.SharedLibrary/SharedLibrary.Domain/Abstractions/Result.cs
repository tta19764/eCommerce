using System.Diagnostics.CodeAnalysis;

namespace SharedLibrary.Domain.Abstractions;

/// <summary>
/// Represents the outcome of an operation that can either succeed or fail with an error.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new result instance.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation succeeded.</param>
    /// <param name="error">The error describing the failure, or <see cref="Error.None"/> for success.</param>
    protected Result(bool isSuccess, Error error)
    {
        // A result is valid only when success has no error and failure has a concrete error.
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException();
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException();
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the failure error, or <see cref="Error.None"/> when the operation succeeded.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful result without a value.</returns>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="error">The error describing the failure.</param>
    /// <returns>A failed result containing the supplied error.</returns>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <param name="value">The successful operation value.</param>
    /// <typeparam name="TValue">The type of value returned by the operation.</typeparam>
    /// <returns>A successful result containing the supplied value.</returns>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    /// <summary>
    /// Creates a failed result with a typed value slot.
    /// </summary>
    /// <param name="error">The error describing the failure.</param>
    /// <typeparam name="TValue">The type of value that would be returned on success.</typeparam>
    /// <returns>A failed typed result containing the supplied error.</returns>
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    /// <summary>
    /// Creates a successful typed result when the value is not null; otherwise creates a null-value failure.
    /// </summary>
    /// <param name="value">The value used to create the result.</param>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <returns>A successful result for non-null values; otherwise, a failed result.</returns>
    public static Result<TValue> Create<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
}

/// <summary>
/// Represents the outcome of an operation that can either return a value or fail with an error.
/// </summary>
/// <typeparam name="TValue">The type of value returned by the operation.</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    /// <summary>
    /// Initializes a new typed result instance.
    /// </summary>
    /// <param name="value">The successful operation value, or the default value for failures.</param>
    /// <param name="isSuccess">Indicates whether the operation succeeded.</param>
    /// <param name="error">The error describing the failure, or <see cref="Error.None"/> for success.</param>
    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the successful operation value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the value is accessed on a failed result.</exception>
    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    /// <summary>
    /// Converts a nullable value into a typed result.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A successful result when the value is not null; otherwise, a failed result.</returns>
    public static implicit operator Result<TValue>(TValue? value) => Create(value);
}
