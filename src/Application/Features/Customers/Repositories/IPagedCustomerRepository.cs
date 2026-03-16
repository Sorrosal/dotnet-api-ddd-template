namespace DotnetApiDddTemplate.Application.Features.Customers.Repositories;

/// <summary>
/// Application-level repository interface extending domain repository
/// with pagination and specification support.
/// </summary>
public interface IPagedCustomerRepository : ICustomerRepository
{
    /// <summary>
    /// Get customers matching specification with pagination.
    /// </summary>
    Task<PagedList<Customer>> GetPagedAsync(
        ISpecification<Customer> specification,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
