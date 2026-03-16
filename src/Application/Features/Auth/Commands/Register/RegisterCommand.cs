namespace DotnetApiDddTemplate.Application.Features.Auth.Commands.Register;

/// <summary>
/// Command to register a new user.
/// </summary>
public sealed record RegisterCommand(
    string Email,
    string Password,
    string? FirstName = null,
    string? LastName = null) : IRequest<Result<string>>;
