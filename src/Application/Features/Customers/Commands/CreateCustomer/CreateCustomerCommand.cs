namespace DotnetApiDddTemplate.Application.Features.Customers.Commands.CreateCustomer;

/// <summary>
/// Command to create a new customer.
/// </summary>
public sealed record CreateCustomerCommand(
    string Name,
    string Email,
    string? PhoneNumber = null,
    string? Address = null,
    string? City = null,
    string? Country = null) : IRequest<Result<Guid>>;
