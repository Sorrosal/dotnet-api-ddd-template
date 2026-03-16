namespace DotnetApiDddTemplate.Api.Requests.Auth;

/// <summary>
/// Request model for user login.
/// </summary>
public sealed record LoginRequest(string Email, string Password);
