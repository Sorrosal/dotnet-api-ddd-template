namespace DotnetApiDddTemplate.Application.Features.Customers.Queries.GetCustomerById;

/// <summary>
/// Response DTO for GetCustomerByIdQuery.
/// </summary>
public sealed record GetCustomerByIdResponse(
    Guid Id,
    string Name,
    string Email,
    string? PhoneNumber,
    string? Address,
    string? City,
    string? Country,
    DateTime CreatedAtUtc,
    string? CreatedBy,
    DateTime? ModifiedAtUtc,
    string? ModifiedBy);
