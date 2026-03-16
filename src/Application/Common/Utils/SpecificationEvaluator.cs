namespace DotnetApiDddTemplate.Application.Common.Utils;

/// <summary>
/// Evaluates specifications against a queryable source.
/// Applies filtering and ordering from the specification.
/// </summary>
public static class SpecificationEvaluator<T>
    where T : class
{
    /// <summary>
    /// Get a queryable with specification applied.
    /// </summary>
    public static IQueryable<T> GetQuery(
        IQueryable<T> inputQuery,
        ISpecification<T> specification)
    {
        var query = inputQuery;

        // Use the query from specification if it has been set
        if (specification.Query != null)
        {
            query = specification.Query;
        }

        return query;
    }
}
