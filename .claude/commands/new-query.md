---
description: Scaffold a new CQRS query with Specification Pattern support, handler, response DTO, ILogger, and Result pattern
allowed-tools: Read, Write, Edit, Glob, Grep, Bash
argument-hint: <BoundedContext> <QueryName>
---

# Scaffold New CQRS Query with Best Practices

Parse `$ARGUMENTS` to extract `BoundedContext` and `QueryName`. If missing, ask the user.

Follow ALL patterns in `CLAUDE.md`, `PROJECT_STRUCTURE.md`, and `BEST_PRACTICES.md`.

---

## Files to Create

### 1. Query Response DTO
**Path:** `src/Application/Features/{BoundedContext}/Queries/{QueryName}/{QueryName}Response.cs`

```csharp
namespace DotnetApiDddTemplate.Application.Features.{BoundedContext}.Queries.{QueryName};

/// <summary>
/// Response DTO for {QueryName} query.
/// Maps domain entities to safe, read-only transfer objects.
/// </summary>
public sealed record {QueryName}Response(
    Guid Id,
    string Name,
    string Description);
```

---

### 2. Query Record
**Path:** `src/Application/Features/{BoundedContext}/Queries/{QueryName}/{QueryName}Query.cs`

```csharp
namespace DotnetApiDddTemplate.Application.Features.{BoundedContext}.Queries.{QueryName};

/// <summary>
/// Query to fetch {AggregateName}.
/// Returns Result<T> for explicit success/failure handling.
/// For list queries, use Result<PagedList<{QueryName}Response>>.
/// </summary>
public sealed record {QueryName}Query(Guid Id) : IRequest<Result<{QueryName}Response>>;

// Example: List query with pagination and filtering
// public sealed record Get{AggregateName}ListQuery(
//     int PageNumber = 1,
//     int PageSize = 10,
//     string? SearchTerm = null) : IRequest<Result<PagedList<{QueryName}Response>>>;
```

---

### 3. Query Validator (if query has parameters)
**Path:** `src/Application/Features/{BoundedContext}/Queries/{QueryName}/{QueryName}QueryValidator.cs`

```csharp
namespace DotnetApiDddTemplate.Application.Features.{BoundedContext}.Queries.{QueryName};

/// <summary>
/// Validator for {QueryName}Query parameters.
/// Validates IDs, pagination, and filter parameters.
/// </summary>
public sealed class {QueryName}QueryValidator : AbstractValidator<{QueryName}Query>
{
    public {QueryName}QueryValidator()
    {
        RuleFor(x => x.Id)
            .ValidateId();
    }
}

// Example: List query validator
// public sealed class Get{AggregateName}ListQueryValidator : AbstractValidator<Get{AggregateName}ListQuery>
// {
//     public Get{AggregateName}ListQueryValidator()
//     {
//         RuleFor(x => x.PageNumber).GreaterThan(0);
//         RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
//     }
// }
```

---

### 4. Query Handler
**Path:** `src/Application/Features/{BoundedContext}/Queries/{QueryName}/{QueryName}QueryHandler.cs`

```csharp
namespace DotnetApiDddTemplate.Application.Features.{BoundedContext}.Queries.{QueryName};

/// <summary>
/// Handler for {QueryName} query.
/// Read-only operation - queries NEVER modify state.
/// Uses Specification Pattern for complex filtering.
///
/// Injected dependencies:
/// - IRepository<T> for data access (queries only)
/// - ILogger<T> for structured logging
/// - ICurrentUser for authorization/filtering by user context
/// </summary>
public sealed class {QueryName}QueryHandler(
    I{AggregateName}Repository {aggregateRepository},
    ILogger<{QueryName}QueryHandler> logger)
    : IRequestHandler<{QueryName}Query, Result<{QueryName}Response>>
{
    public async Task<Result<{QueryName}Response>> Handle(
        {QueryName}Query request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Fetching {AggregateName} - Id: {Id}",
            "{AggregateName}",
            request.Id);

        // Single entity fetch
        var {aggregate} = await {aggregateRepository}.GetByIdAsync(
            new {AggregateName}Id(request.Id),
            cancellationToken);

        if ({aggregate} is null)
        {
            logger.LogWarning(
                "{AggregateName} not found - Id: {Id}",
                "{AggregateName}",
                request.Id);

            return Result<{QueryName}Response>.Failure({AggregateName}Errors.NotFound);
        }

        var response = new {QueryName}Response(
            {aggregate}.Id.Value,
            {aggregate}.Name,
            {aggregate}.Description);

        logger.LogInformation(
            "Successfully fetched {AggregateName} - Id: {Id}",
            "{AggregateName}",
            request.Id);

        return Result<{QueryName}Response>.Success(response);
    }
}

// Example: List query handler with Specification Pattern
// public sealed class Get{AggregateName}ListQueryHandler(
//     I{AggregateName}Repository {aggregateRepository},
//     ILogger<Get{AggregateName}ListQueryHandler> logger)
//     : IRequestHandler<Get{AggregateName}ListQuery, Result<PagedList<{QueryName}Response>>>
// {
//     public async Task<Result<PagedList<{QueryName}Response>>> Handle(
//         Get{AggregateName}ListQuery request,
//         CancellationToken cancellationToken)
//     {
//         logger.LogInformation("Fetching {AggregateName}s - Page: {PageNumber}", "{AggregateName}", request.PageNumber);
//
//         // Use Specification Pattern for reusable, composable queries
//         var specification = new {AggregateName}ByStatusSpecification(
//             request.SearchTerm,
//             request.PageNumber,
//             request.PageSize);
//
//         var {aggregates} = await {aggregateRepository}.GetBySpecificationAsync(specification, cancellationToken);
//         var count = await {aggregateRepository}.CountAsync(
//             new {AggregateName}ByStatusSpecification(request.SearchTerm),
//             cancellationToken);
//
//         var responses = {aggregates}
//             .Select(x => new {QueryName}Response(x.Id.Value, x.Name, x.Description))
//             .ToList();
//
//         var pagedList = new PagedList<{QueryName}Response>(
//             responses,
//             count,
//             request.PageNumber,
//             request.PageSize);
//
//         logger.LogInformation("Fetched {Count} {AggregateName}s", responses.Count, "{AggregateName}");
//         return Result<PagedList<{QueryName}Response>>.Success(pagedList);
//     }
// }
```

---

## Integration in Controller

**Path:** `src/Api/Controllers/V1/{BoundedContext}Controller.cs` (example excerpt)

```csharp
/// <summary>
/// Gets a {AggregateName} by ID.
/// </summary>
/// <param name="id">The {AggregateName} ID</param>
/// <param name="ct">Cancellation token</param>
/// <returns>The {AggregateName} details</returns>
/// <response code="200">Found</response>
/// <response code="404">Not found</response>
[HttpGet("{id}")]
[ProduceResponseType(typeof({QueryName}Response), StatusCodes.Status200OK)]
[ProduceResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
public async Task<IActionResult> GetById(
    Guid id,
    CancellationToken ct)
{
    logger.LogInformation("GET /api/v1/{resource}/{id} - Id: {AggregateId}", id);

    var query = new {QueryName}Query(id);
    var result = await mediator.Send(query, ct);

    if (result.IsFailure)
    {
        logger.LogWarning("Not found - Id: {AggregateId}", id);
        return NotFound(new ErrorResponse(result.Error.Code, result.Error.Message, HttpContext.TraceIdentifier));
    }

    logger.LogInformation("Successfully retrieved {AggregateName} - Id: {AggregateId}", "{AggregateName}", id);
    return Ok(result.Value);
}

/// <summary>
/// Gets paginated list of {AggregateName}s with optional filtering.
/// </summary>
[HttpGet]
[ProduceResponseType(typeof(PagedList<{QueryName}Response>), StatusCodes.Status200OK)]
public async Task<IActionResult> GetList(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? searchTerm = null,
    CancellationToken ct = default)
{
    logger.LogInformation("GET /api/v1/{resource} - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

    var query = new Get{AggregateName}ListQuery(pageNumber, pageSize, searchTerm);
    var result = await mediator.Send(query, ct);

    return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
}
```

---

## Specification Pattern for Complex Queries

**Path:** `src/Application/Features/{BoundedContext}/Specifications/{SpecName}Specification.cs`

```csharp
namespace DotnetApiDddTemplate.Application.Features.{BoundedContext}.Specifications;

public sealed class {AggregateName}ByStatusSpecification : Specification<{AggregateName}>
{
    public {AggregateName}ByStatusSpecification(
        string? searchTerm,
        int pageNumber,
        int pageSize)
    {
        // Define filter criteria
        if (!string.IsNullOrWhiteSpace(searchTerm))
            Criteria = a => a.Name.Contains(searchTerm) && !a.IsDeleted;
        else
            Criteria = a => !a.IsDeleted;

        // Include related entities
        AddInclude(a => a.Items); // if applicable

        // Sort by newest first
        OrderByDescending = a => a.CreatedAtUtc;

        // Apply pagination
        ApplyPaging(pageNumber, pageSize);
    }
}
```

---

## Key Patterns

✅ **Result<T> Pattern** - No exceptions, explicit success/failure
✅ **Specification Pattern** - Reusable, composable query logic
✅ **Structured Logging** - ILogger<T> with context
✅ **Read-Only** - Never modifies state (no UnitOfWork)
✅ **DTO Mapping** - Domain entities never returned directly
✅ **Pagination** - PagedList<T> for large result sets
✅ **Filtering** - Specification pattern for complex filters
✅ **Soft Delete** - Queries automatically exclude deleted entities (global filter)

---

## Guidelines

- **Queries are read-only** - NEVER inject IUnitOfWork
- **Use Specifications** for complex filtering and pagination
- **Map to DTOs** - Never expose domain entities to API clients
- **Validate parameters** - Validate IDs, pagination, filters
- **Log at Information level** for normal queries
- **Log at Warning level** if entity not found
- **File-scoped namespaces** in all files
- **Verify build**: `dotnet build && dotnet test`

---

## Next Steps

1. Create Response DTO mapping entity properties
2. Create Query record with parameters
3. Create optional Validator for query parameters
4. Create Handler using Specification Pattern for complex queries
5. Add to controller with pagination support
6. Create unit tests - test success/not-found branches
7. Create integration tests - test with real database

---

**Reference:**
- `BEST_PRACTICES.md` - Specification Pattern, Result Pattern, Soft Delete filtering
- `CLAUDE.md` - Query naming conventions
- `SharedRules.cs` - Reusable validation rules
