namespace DotnetApiDddTemplate.Api.Models;

/// <summary>
/// Standard error response model for API.
/// Includes error code, message, and correlation ID for tracing.
/// </summary>
public sealed record ErrorResponse(
    string Code,
    string Message,
    string? TraceId = null);
