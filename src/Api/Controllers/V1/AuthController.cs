namespace DotnetApiDddTemplate.Api.Controllers.V1;

/// <summary>
/// Authentication controller.
/// Provides endpoints for user registration, login, token refresh, and logout.
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController(
    ISender sender,
    ILogger<AuthController> logger) : ControllerBase
{
    /// <summary>
    /// Register a new user.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        var command = new RegisterCommand(request.Email, request.Password, request.FirstName, request.LastName);
        var result = await sender.Send(command, ct);

        if (result.IsFailure)
            return BadRequest(result.Error);

        logger.LogInformation("User registered: {Email}", request.Email);
        return CreatedAtAction(nameof(Register), result.Value);
    }

    /// <summary>
    /// Login user and issue JWT and refresh tokens.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await sender.Send(command, ct);

        if (result.IsFailure)
            return Unauthorized(result.Error);

        logger.LogInformation("User logged in: {Email}", request.Email);
        return Ok(result.Value);
    }

    /// <summary>
    /// Refresh JWT using refresh token.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await sender.Send(command, ct);

        if (result.IsFailure)
            return BadRequest(result.Error);

        logger.LogInformation("Token refreshed");
        return Ok(result.Value);
    }

    /// <summary>
    /// Logout user by revoking refresh token.
    /// </summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken ct)
    {
        var command = new LogoutCommand(request.RefreshToken);
        var result = await sender.Send(command, ct);

        if (result.IsFailure)
            return BadRequest(result.Error);

        logger.LogInformation("User logged out");
        return NoContent();
    }
}
