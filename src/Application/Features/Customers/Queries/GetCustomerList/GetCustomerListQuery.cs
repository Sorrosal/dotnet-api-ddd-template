namespace DotnetApiDddTemplate.Application.Features.Customers.Queries.GetCustomerList;

/// <summary>
/// Query to get paginated list of customers.
/// </summary>
public sealed record GetCustomerListQuery(
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<Result<PagedList<GetCustomerListItemResponse>>>;
