namespace DotnetApiDddTemplate.Api.Requests.Auth;

/// <summary>
/// Request model for user registration.
/// </summary>
public sealed record RegisterRequest(
    string Email,
    string Password,
    string? FirstName = null,
    string? LastName = null);
