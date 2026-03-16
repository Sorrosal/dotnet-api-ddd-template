namespace DotnetApiDddTemplate.Api.Requests;

/// <summary>
/// Request DTO for creating a customer.
/// </summary>
public sealed record CreateCustomerRequest(
    string Name,
    string Email,
    string? PhoneNumber = null,
    string? Address = null,
    string? City = null,
    string? Country = null);
