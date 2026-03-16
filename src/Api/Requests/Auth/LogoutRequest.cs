namespace DotnetApiDddTemplate.Api.Requests.Auth;

/// <summary>
/// Request model for user logout.
/// </summary>
public sealed record LogoutRequest(string RefreshToken);
