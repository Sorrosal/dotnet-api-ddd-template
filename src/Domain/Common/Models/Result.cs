namespace DotnetApiDddTemplate.Domain.Common.Models;

/// <summary>
/// Result pattern for explicit error handling.
/// Encapsulates success/failure state without exceptions for control flow.
/// </summary>
public sealed record Result(bool IsSuccess, Error Error)
{
    public bool IsFailure => !IsSuccess;

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);
}

/// <summary>
/// Generic result pattern with typed value.
/// </summary>
public sealed record Result<T>(bool IsSuccess, T? Value, Error Error)
{
    public bool IsFailure => !IsSuccess;

    public static Result<T> Success(T value) => new(true, value, Error.None);

    public static Result<T> Failure(Error error) => new(false, default, error);

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Error error) => Failure(error);
}
