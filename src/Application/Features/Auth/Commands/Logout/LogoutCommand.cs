namespace DotnetApiDddTemplate.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Command to logout user by revoking refresh token.
/// </summary>
public sealed record LogoutCommand(string RefreshToken) : IRequest<Result>;
