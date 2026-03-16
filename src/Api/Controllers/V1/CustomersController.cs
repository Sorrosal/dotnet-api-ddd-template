namespace DotnetApiDddTemplate.Api.Controllers.V1;

/// <summary>
/// API endpoints for customer management.
/// Provides CRUD operations for customers with full validation, error handling, and logging.
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class CustomersController(
    ISender sender,
    ILogger<CustomersController> logger) : ControllerBase
{
    /// <summary>
    /// Create a new customer.
    /// </summary>
    /// <param name="request">Customer creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created customer ID and location header</returns>
    [HttpPost]

    [Authorize]
    public async Task<IActionResult> CreateCustomer(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.TraceIdentifier;
        using (logger.BeginScope(new Dictionary<string, object> { { "CorrelationId", correlationId } }))
        {
            logger.LogInformation("Creating customer: {Name}", request.Name);

            var command = new CreateCustomerCommand(
                request.Name,
                request.Email,
                request.PhoneNumber,
                request.Address,
                request.City,
                request.Country);

            var result = await sender.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(new ErrorResponse(
                    result.Error.Code,
                    result.Error.Message,
                    correlationId));

            return CreatedAtAction(
                nameof(GetCustomerById),
                new { customerId = result.Value },
                result.Value);
        }
    }

    /// <summary>
    /// Get customer by ID.
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Customer details</returns>
    [HttpGet("{customerId}")]

    public async Task<IActionResult> GetCustomerById(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.TraceIdentifier;
        using (logger.BeginScope(new Dictionary<string, object> { { "CorrelationId", correlationId } }))
        {
            logger.LogInformation("Retrieving customer {CustomerId}", customerId);

            var query = new GetCustomerByIdQuery(customerId);
            var result = await sender.Send(query, cancellationToken);

            if (result.IsFailure)
                return NotFound(new ErrorResponse(
                    result.Error.Code,
                    result.Error.Message,
                    correlationId));

            return Ok(result.Value);
        }
    }

    /// <summary>
    /// Get paginated list of customers.
    /// </summary>
    /// <param name="searchTerm">Optional search term (name or email)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of customers</returns>
    [HttpGet]

    public async Task<IActionResult> GetCustomers(
        [FromQuery] string? searchTerm = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var correlationId = HttpContext.TraceIdentifier;
        using (logger.BeginScope(new Dictionary<string, object> { { "CorrelationId", correlationId } }))
        {
            logger.LogInformation(
                "Retrieving customers: PageNumber={PageNumber}, PageSize={PageSize}, SearchTerm={SearchTerm}",
                pageNumber,
                pageSize,
                searchTerm ?? "none");

            var query = new GetCustomerListQuery(searchTerm, pageNumber, pageSize);
            var result = await sender.Send(query, cancellationToken);

            if (result.IsFailure)
                return BadRequest(new ErrorResponse(
                    result.Error.Code,
                    result.Error.Message,
                    correlationId));

            return Ok(result.Value);
        }
    }

    /// <summary>
    /// Update an existing customer.
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="request">Customer update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpPut("{customerId}")]

    [Authorize]
    public async Task<IActionResult> UpdateCustomer(
        Guid customerId,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.TraceIdentifier;
        using (logger.BeginScope(new Dictionary<string, object> { { "CorrelationId", correlationId } }))
        {
            logger.LogInformation("Updating customer {CustomerId}", customerId);

            var command = new UpdateCustomerCommand(
                customerId,
                request.Name,
                request.Email,
                request.PhoneNumber,
                request.Address,
                request.City,
                request.Country);

            var result = await sender.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(new ErrorResponse(
                    result.Error.Code,
                    result.Error.Message,
                    correlationId));

            return NoContent();
        }
    }

    /// <summary>
    /// Delete a customer (soft delete).
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{customerId}")]

    [Authorize]
    public async Task<IActionResult> DeleteCustomer(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.TraceIdentifier;
        using (logger.BeginScope(new Dictionary<string, object> { { "CorrelationId", correlationId } }))
        {
            logger.LogInformation("Deleting customer {CustomerId}", customerId);

            var command = new DeleteCustomerCommand(customerId);
            var result = await sender.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(new ErrorResponse(
                    result.Error.Code,
                    result.Error.Message,
                    correlationId));

            return NoContent();
        }
    }
}
