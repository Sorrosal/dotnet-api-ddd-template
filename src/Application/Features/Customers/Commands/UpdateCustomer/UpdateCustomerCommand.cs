namespace DotnetApiDddTemplate.Application.Features.Customers.Commands.UpdateCustomer;

/// <summary>
/// Command to update an existing customer.
/// </summary>
public sealed record UpdateCustomerCommand(
    Guid CustomerId,
    string Name,
    string Email,
    string? PhoneNumber = null,
    string? Address = null,
    string? City = null,
    string? Country = null) : IRequest<Result>;
