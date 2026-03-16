namespace DotnetApiDddTemplate.Application.Features.Customers.Queries.GetCustomerList;

/// <summary>
/// Handler for GetCustomerListQuery.
/// Returns paginated list of customers with optional search filtering.
/// </summary>
public sealed class GetCustomerListQueryHandler(
    IPagedCustomerRepository customerRepository,
    ILogger<GetCustomerListQueryHandler> logger) : IRequestHandler<GetCustomerListQuery, Result<PagedList<GetCustomerListItemResponse>>>
{
    public async Task<Result<PagedList<GetCustomerListItemResponse>>> Handle(
        GetCustomerListQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Retrieving customer list: PageNumber={PageNumber}, PageSize={PageSize}, SearchTerm={SearchTerm}",
            request.PageNumber,
            request.PageSize,
            request.SearchTerm ?? "none");

        try
        {
            // Create specification with search filter
            var specification = new CustomersBySearchSpecification(request.SearchTerm);

            // Get paged results using specification
            var pagedCustomers = await customerRepository.GetPagedAsync(
                specification,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            // Map to response DTOs
            var items = pagedCustomers.Items
                .Select(c => new GetCustomerListItemResponse(
                    c.Id.Value,
                    c.Name,
                    c.Email,
                    c.City,
                    c.CreatedAtUtc))
                .ToList();

            var response = new PagedList<GetCustomerListItemResponse>(
                items,
                pagedCustomers.TotalCount,
                request.PageNumber,
                request.PageSize);

            logger.LogInformation(
                "Retrieved {Count} customers (total: {Total})",
                items.Count,
                pagedCustomers.TotalCount);

            return Result<PagedList<GetCustomerListItemResponse>>.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while retrieving customer list");
            throw;
        }
    }
}
