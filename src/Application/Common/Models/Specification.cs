namespace DotnetApiDddTemplate.Application.Common.Models;

/// <summary>
/// Base specification implementation.
/// Provides reusable query logic with filters, ordering, and includes.
/// </summary>
public abstract class Specification<T> : ISpecification<T>
    where T : class
{
    /// <summary>
    /// The base queryable - can be modified by derived specifications.
    /// </summary>
    protected IQueryable<T> _query = null!;

    /// <summary>
    /// Get the query.
    /// </summary>
    public IQueryable<T> Query => _query;

    /// <summary>
    /// Set the base query (called by derived class constructor).
    /// </summary>
    protected void SetQuery(IQueryable<T> query)
    {
        _query = query;
    }
}
