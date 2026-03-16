namespace DotnetApiDddTemplate.Api.Requests;

/// <summary>
/// Request DTO for updating a customer.
/// </summary>
public sealed record UpdateCustomerRequest(
    string Name,
    string Email,
    string? PhoneNumber = null,
    string? Address = null,
    string? City = null,
    string? Country = null);
