namespace DotnetApiDddTemplate.Application.Features.Customers.Commands.DeleteCustomer;

/// <summary>
/// Command to delete (soft delete) a customer.
/// </summary>
public sealed record DeleteCustomerCommand(Guid CustomerId) : IRequest<Result>;
