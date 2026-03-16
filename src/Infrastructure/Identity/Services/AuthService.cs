namespace DotnetApiDddTemplate.Infrastructure.Identity.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Implementation of IAuthService.
/// Handles user registration, login, token refresh, and logout using ASP.NET Identity and JWT.
/// </summary>
public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ApplicationDbContext dbContext,
    JwtOptions jwtOptions,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<Result<string>> RegisterAsync(
        string email,
        string password,
        string? firstName,
        string? lastName,
        CancellationToken ct = default)
    {
        try
        {
            // Check if email already exists
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser is not null)
                return Result<string>.Failure(AuthErrors.EmailAlreadyExists);

            // Create new user
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };

            // Create user with password
            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                logger.LogWarning("Failed to create user {Email}: {Errors}", email, string.Join("; ", createResult.Errors.Select(e => e.Description)));
                return Result<string>.Failure(AuthErrors.RegistrationFailed);
            }

            // Assign "User" role
            await userManager.AddToRoleAsync(user, "User");

            logger.LogInformation("User registered successfully: {Email}", email);
            return Result<string>.Success(user.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during user registration for {Email}", email);
            return Result<string>.Failure(AuthErrors.RegistrationFailed);
        }
    }

    public async Task<Result<AuthResponse>> LoginAsync(
        string email,
        string password,
        CancellationToken ct = default)
    {
        try
        {
            // Find user by email
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
                return Result<AuthResponse>.Failure(AuthErrors.InvalidCredentials);

            // Verify password
            var passwordValid = await userManager.CheckPasswordAsync(user, password);
            if (!passwordValid)
                return Result<AuthResponse>.Failure(AuthErrors.InvalidCredentials);

            // Get user roles
            var roles = await userManager.GetRolesAsync(user);

            // Generate JWT token
            var accessToken = GenerateJwtToken(user, roles);
            var refreshToken = GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddMinutes(jwtOptions.ExpiryMinutes);

            // Persist refresh token
            var refreshTokenEntity = new ApplicationRefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(jwtOptions.RefreshTokenExpiryDays),
                IsRevoked = false,
                CreatedAtUtc = DateTime.UtcNow
            };

            dbContext.RefreshTokens.Add(refreshTokenEntity);
            await dbContext.SaveChangesAsync(ct);

            logger.LogInformation("User logged in successfully: {Email}", email);

            return Result<AuthResponse>.Success(new AuthResponse(
                accessToken,
                refreshToken,
                expiresAt,
                user.Id,
                user.Email ?? string.Empty));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during login for {Email}", email);
            return Result<AuthResponse>.Failure(AuthErrors.InvalidCredentials);
        }
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken ct = default)
    {
        try
        {
            // Find refresh token
            var token = await dbContext.RefreshTokens
                .FirstOrDefaultAsync(
                    t => t.Token == refreshToken && !t.IsRevoked && t.ExpiresAtUtc > DateTime.UtcNow,
                    ct);

            if (token is null)
                return Result<AuthResponse>.Failure(AuthErrors.InvalidRefreshToken);

            // Get user
            var user = await userManager.FindByIdAsync(token.UserId);
            if (user is null)
                return Result<AuthResponse>.Failure(AuthErrors.InvalidRefreshToken);

            // Mark old token as revoked
            token.IsRevoked = true;

            // Get user roles
            var roles = await userManager.GetRolesAsync(user);

            // Generate new tokens
            var accessToken = GenerateJwtToken(user, roles);
            var newRefreshToken = GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddMinutes(jwtOptions.ExpiryMinutes);

            // Persist new refresh token
            var newRefreshTokenEntity = new ApplicationRefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = newRefreshToken,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(jwtOptions.RefreshTokenExpiryDays),
                IsRevoked = false,
                CreatedAtUtc = DateTime.UtcNow
            };

            dbContext.RefreshTokens.Add(newRefreshTokenEntity);
            await dbContext.SaveChangesAsync(ct);

            logger.LogInformation("Token refreshed for user: {UserId}", user.Id);

            return Result<AuthResponse>.Success(new AuthResponse(
                accessToken,
                newRefreshToken,
                expiresAt,
                user.Id,
                user.Email ?? string.Empty));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during token refresh");
            return Result<AuthResponse>.Failure(AuthErrors.InvalidRefreshToken);
        }
    }

    public async Task<Result> LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        try
        {
            // Find refresh token
            var token = await dbContext.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken, ct);

            if (token is not null)
            {
                // Mark as revoked
                token.IsRevoked = true;
                await dbContext.SaveChangesAsync(ct);
                logger.LogInformation("User logged out, token revoked");
            }

            // Idempotent: return success even if token not found
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during logout");
            return Result.Failure(new Error("Auth.LogoutFailed", "Logout failed"));
        }
    }

    /// <summary>
    /// Generate JWT token for user.
    /// </summary>
    private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("sub", user.Id)
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtOptions.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generate secure refresh token (base64 encoded random bytes).
    /// </summary>
    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }

        return Convert.ToBase64String(randomNumber);
    }
}
