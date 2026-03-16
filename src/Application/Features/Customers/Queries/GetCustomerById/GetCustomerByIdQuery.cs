namespace DotnetApiDddTemplate.Application.Features.Customers.Queries.GetCustomerById;

/// <summary>
/// Query to get a customer by ID.
/// </summary>
public sealed record GetCustomerByIdQuery(Guid CustomerId) : IRequest<Result<GetCustomerByIdResponse>>;
