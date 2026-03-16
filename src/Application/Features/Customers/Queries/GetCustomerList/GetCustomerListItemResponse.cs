namespace DotnetApiDddTemplate.Application.Features.Customers.Queries.GetCustomerList;

/// <summary>
/// Response DTO for customer list item.
/// </summary>
public sealed record GetCustomerListItemResponse(
    Guid Id,
    string Name,
    string Email,
    string? City,
    DateTime CreatedAtUtc);
