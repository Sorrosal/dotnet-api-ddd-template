---
description: Scaffold a new CQRS query with handler, response DTO, and optional validator
allowed-tools: Read, Write, Edit, Glob, Grep
argument-hint: <FeatureName> <QueryName>
---

# Scaffold New CQRS Query

Parse `$ARGUMENTS` to extract `FeatureName` and `QueryName`. If missing, ask the user.

Follow ALL patterns and conventions defined in the root `CLAUDE.md`. Read it first.

## Files to Create

### 1. Response DTO
**Path:** `src/Application/Features/{FeatureName}/Queries/{QueryName}/{QueryName}Response.cs`
```csharp
namespace DotnetApiDddTemplate.Application.Features.{FeatureName}.Queries.{QueryName};

public sealed record {QueryName}Response(
    // Map relevant entity properties to the response
);
```

### 2. Query Record
**Path:** `src/Application/Features/{FeatureName}/Queries/{QueryName}/{QueryName}Query.cs`
```csharp
namespace DotnetApiDddTemplate.Application.Features.{FeatureName}.Queries.{QueryName};

public sealed record {QueryName}Query(
    // Add filter/id parameters as needed
) : IRequest<Result<{QueryName}Response>>;
```
- For list queries, return `Result<PagedList<{QueryName}Response>>` and accept `int Page, int PageSize` parameters.

### 3. Query Handler
**Path:** `src/Application/Features/{FeatureName}/Queries/{QueryName}/{QueryName}QueryHandler.cs`
```csharp
namespace DotnetApiDddTemplate.Application.Features.{FeatureName}.Queries.{QueryName};

public sealed class {QueryName}QueryHandler(
    I{AggregateRepository} repository) : IRequestHandler<{QueryName}Query, Result<{QueryName}Response>>
{
    public async Task<Result<{QueryName}Response>> Handle(
        {QueryName}Query request,
        CancellationToken cancellationToken)
    {
        // 1. Query repository (read-only)
        // 2. Map entity to response DTO
        // 3. Return Result.Success(response) or Result.Failure(NotFound)
    }
}
```

### 4. Query Validator (if query has parameters)
**Path:** `src/Application/Features/{FeatureName}/Queries/{QueryName}/{QueryName}QueryValidator.cs`
- Validate pagination params (Page > 0, PageSize between 1-100).
- Validate IDs are not empty Guid.

## Guidelines

- Queries are **read-only** — NEVER call `unitOfWork.SaveChangesAsync`.
- Do NOT inject `IUnitOfWork` in query handlers.
- Use primary constructors for the handler.
- Map entities to response DTOs — never return domain entities from queries.
- File-scoped namespaces in all files.
- Verify build: `dotnet build`.
