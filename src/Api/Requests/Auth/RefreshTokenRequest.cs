namespace DotnetApiDddTemplate.Api.Requests.Auth;

/// <summary>
/// Request model for token refresh.
/// </summary>
public sealed record RefreshTokenRequest(string RefreshToken);
