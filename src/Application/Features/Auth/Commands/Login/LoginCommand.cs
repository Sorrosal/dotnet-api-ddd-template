namespace DotnetApiDddTemplate.Application.Features.Auth.Commands.Login;

/// <summary>
/// Command to login a user and issue tokens.
/// </summary>
public sealed record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponse>>;
