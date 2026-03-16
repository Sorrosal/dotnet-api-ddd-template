namespace DotnetApiDddTemplate.Application.Features.Customers.Queries.Specifications;

/// <summary>
/// Specification for filtering customers by search term.
/// Searches by name and email.
/// Used with SpecificationEvaluator for reusable query logic.
/// </summary>
public sealed class CustomersBySearchSpecification : Specification<Customer>
{
    private readonly string? _searchTerm;

    public CustomersBySearchSpecification(string? searchTerm = null)
    {
        _searchTerm = searchTerm;
    }

    /// <summary>
    /// Apply the specification to a queryable.
    /// This allows the handler to apply search filtering dynamically.
    /// </summary>
    public IQueryable<Customer> Apply(IQueryable<Customer> query)
    {
        if (!string.IsNullOrWhiteSpace(_searchTerm))
        {
            var lowerSearchTerm = _searchTerm.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(lowerSearchTerm) ||
                c.Email.ToLower().Contains(lowerSearchTerm));
        }

        return query.OrderBy(c => c.Name);
    }
}

