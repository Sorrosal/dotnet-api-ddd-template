namespace DotnetApiDddTemplate.Domain.Common.Models;

/// <summary>
/// Represents an error with code and message.
/// Used in Result<T> pattern for explicit error handling.
/// </summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static readonly Error NullValue = new(
        "Error.NullValue",
        "Null value was provided");

    public static readonly Error ConcurrencyConflict = new(
        "Concurrency.Conflict",
        "A concurrency conflict occurred");
}
