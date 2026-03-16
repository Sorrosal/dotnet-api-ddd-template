namespace DotnetApiDddTemplate.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Command to refresh JWT using a refresh token.
/// </summary>
public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthResponse>>;
