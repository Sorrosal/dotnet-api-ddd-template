namespace DotnetApiDddTemplate.Application.Common.Interfaces;

/// <summary>
/// Specification pattern interface.
/// Encapsulates reusable query logic.
/// </summary>
public interface ISpecification<T>
{
    /// <summary>
    /// The base queryable to be evaluated.
    /// </summary>
    IQueryable<T> Query { get; }
}
