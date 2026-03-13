---
description: Scaffold a complete bounded context/module with aggregate, CRUD operations, versioned controller, domain events, and tests
allowed-tools: Read, Write, Edit, Glob, Grep, Bash
argument-hint: <ModuleName> <AggregateName>
---

# Scaffold New Bounded Context Module

Parse `$ARGUMENTS` to extract `ModuleName` (BoundedContext name) and `AggregateName`. If missing, ask the user.

Follow ALL patterns and conventions defined in the root `CLAUDE.md`. Read it first.

This command scaffolds an entire production-ready module. Execute each step in sequence.

---

## Step 1 — Domain Layer

### 1.1 Strongly Typed ID
`src/Domain/{ModuleName}/ValueObjects/{AggregateName}Id.cs`
```csharp
public readonly record struct {AggregateName}Id(Guid Value) : IStronglyTypedId;
```

### 1.2 Domain Errors
`src/Domain/{ModuleName}/Errors/{AggregateName}Errors.cs`
```csharp
public static class {AggregateName}Errors
{
    public static readonly Error NotFound = new("{ModuleName}.{AggregateName}.NotFound", "...");
    public static readonly Error NameTooLong = new("{ModuleName}.{AggregateName}.NameTooLong", "...");
}
```

### 1.3 Domain Events
`src/Domain/{ModuleName}/Events/{AggregateName}CreatedDomainEvent.cs`
`src/Domain/{ModuleName}/Events/{AggregateName}UpdatedDomainEvent.cs`
`src/Domain/{ModuleName}/Events/{AggregateName}DeletedDomainEvent.cs`

### 1.4 Aggregate Root
`src/Domain/{ModuleName}/Entities/{AggregateName}.cs`
- Sealed class extending `AggregateRoot<{AggregateName}Id>`.
- Private EF Core constructor.
- `static Result<{AggregateName}> Create(string name, ...)` → raises `{AggregateName}CreatedDomainEvent`.
- `Result Update(string name, ...)` → raises `{AggregateName}UpdatedDomainEvent`.
- `Result Delete()` → sets soft-delete flag or raises `{AggregateName}DeletedDomainEvent`.
- Properties: `Name`, `CreatedAt`, `UpdatedAt` (at minimum).

### 1.5 Repository Interface
`src/Domain/{ModuleName}/Repositories/I{AggregateName}Repository.cs`
- Extends `IRepository<{AggregateName}, {AggregateName}Id>`.
- Add: `Task<IReadOnlyList<{AggregateName}>> GetPagedAsync(int page, int pageSize, CancellationToken ct)`.
- Add: `Task<int> CountAsync(CancellationToken ct)`.

---

## Step 2 — Application Layer (CQRS)

### 2.1 Commands
Create command + handler + validator for each:

**Create:** `src/Application/Features/{ModuleName}/Commands/Create{AggregateName}/`
- Command: `Create{AggregateName}Command(string Name, ...) : IRequest<Result<{AggregateName}Id>>`
- Handler: load none, call `{AggregateName}.Create(...)`, `repository.Add(entity)`, `unitOfWork.SaveChangesAsync`.
- Validator: validate all input fields.

**Update:** `src/Application/Features/{ModuleName}/Commands/Update{AggregateName}/`
- Command: `Update{AggregateName}Command({AggregateName}Id Id, string Name, ...) : IRequest<Result>`
- Handler: load by ID (return NotFound if null), call `entity.Update(...)`, `unitOfWork.SaveChangesAsync`.
- Validator: validate ID not empty, validate fields.

**Delete:** `src/Application/Features/{ModuleName}/Commands/Delete{AggregateName}/`
- Command: `Delete{AggregateName}Command({AggregateName}Id Id) : IRequest<Result>`
- Handler: load by ID, call `entity.Delete()`, `unitOfWork.SaveChangesAsync`.
- Validator: validate ID not empty.

### 2.2 Queries
**GetById:** `src/Application/Features/{ModuleName}/Queries/Get{AggregateName}ById/`
- Query: `Get{AggregateName}ByIdQuery({AggregateName}Id Id) : IRequest<Result<{AggregateName}Response>>`
- Response: `{AggregateName}Response(Guid Id, string Name, ...)`.
- Handler: load by ID, map to response DTO, return NotFound if null.

**GetList:** `src/Application/Features/{ModuleName}/Queries/Get{AggregateName}List/`
- Query: `Get{AggregateName}ListQuery(int Page = 1, int PageSize = 20) : IRequest<Result<PagedList<{AggregateName}Response>>>`
- Handler: call `GetPagedAsync` + `CountAsync`, map to `PagedList<{AggregateName}Response>`.

### 2.3 Domain Event Handler
`src/Application/Features/{ModuleName}/Events/{AggregateName}CreatedDomainEventHandler.cs`
- `INotificationHandler<{AggregateName}CreatedDomainEvent>`.
- Log the event at minimum. Expand as needed.

---

## Step 3 — Infrastructure Layer

### 3.1 EF Core Configuration
`src/Infrastructure/Persistence/Configurations/{AggregateName}Configuration.cs`
- `IEntityTypeConfiguration<{AggregateName}>`.
- Configure typed ID conversion.
- Configure table name as `{AggregateName}s`.
- Configure property constraints.

### 3.2 Repository Implementation
`src/Infrastructure/Persistence/Repositories/{AggregateName}Repository.cs`
- Implements `I{AggregateName}Repository`.
- Primary constructor injecting `ApplicationDbContext`.
- Implement all interface methods.

### 3.3 Register in DbContext and DI
- Add `public DbSet<{AggregateName}> {AggregateName}s => Set<{AggregateName}>();` to `ApplicationDbContext`.
- Register `I{AggregateName}Repository` → `{AggregateName}Repository` in DI container.

---

## Step 4 — API Layer

### 4.1 Controller
`src/Api/Controllers/V1/{AggregateName}sController.cs`
```csharp
[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class {AggregateName}sController(ISender sender) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) { ... }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) { ... }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Create{AggregateName}Request request, CancellationToken ct) { ... }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Update{AggregateName}Request request, CancellationToken ct) { ... }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { ... }
}
```
- Map `Result` failures to appropriate HTTP status codes using the global error handling middleware.
- Request records: `Create{AggregateName}Request`, `Update{AggregateName}Request` in `src/Api/Contracts/`.

---

## Step 5 — Migration

Run: `dotnet ef migrations add Add{ModuleName}Module --project src/Infrastructure --startup-project src/Api`

Verify the generated migration looks correct.

---

## Step 6 — Tests

### 6.1 Unit Tests
`tests/UnitTests/Domain/{ModuleName}/{AggregateName}Tests.cs`
- Test `Create`: valid, invalid name, etc.
- Test `Update`: valid, domain error.
- Test domain events are raised.

`tests/UnitTests/Application/{ModuleName}/Commands/Create{AggregateName}CommandHandlerTests.cs`
- Mock repository + IUnitOfWork.
- Test success path, test entity not found.

### 6.2 Integration Tests
`tests/IntegrationTests/Api/{ModuleName}/{AggregateName}sTests.cs`
- Tests for all 5 endpoints: GET by ID, GET list, POST, PUT, DELETE.
- Cover: 200/201, 400 validation, 404 not found.

---

## Final Verification

```bash
dotnet build
dotnet test
```

All tests must pass before the scaffold is considered complete.
