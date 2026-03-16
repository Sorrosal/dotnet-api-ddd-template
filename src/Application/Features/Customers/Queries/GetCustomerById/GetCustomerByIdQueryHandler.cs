namespace DotnetApiDddTemplate.Application.Features.Customers.Queries.GetCustomerById;

/// <summary>
/// Handler for GetCustomerByIdQuery.
/// </summary>
public sealed class GetCustomerByIdQueryHandler(
    ICustomerRepository customerRepository,
    ILogger<GetCustomerByIdQueryHandler> logger) : IRequestHandler<GetCustomerByIdQuery, Result<GetCustomerByIdResponse>>
{
    public async Task<Result<GetCustomerByIdResponse>> Handle(
        GetCustomerByIdQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving customer {CustomerId}", request.CustomerId);

        var customerId = new CustomerId(request.CustomerId);
        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);

        if (customer is null)
        {
            logger.LogWarning("Customer {CustomerId} not found", request.CustomerId);
            return Result<GetCustomerByIdResponse>.Failure(CustomerErrors.NotFound);
        }

        var response = new GetCustomerByIdResponse(
            customer.Id.Value,
            customer.Name,
            customer.Email,
            customer.PhoneNumber,
            customer.Address,
            customer.City,
            customer.Country,
            customer.CreatedAtUtc,
            customer.CreatedBy,
            customer.ModifiedAtUtc,
            customer.ModifiedBy);

        logger.LogInformation("Customer {CustomerId} retrieved successfully", request.CustomerId);

        return Result<GetCustomerByIdResponse>.Success(response);
    }
}
