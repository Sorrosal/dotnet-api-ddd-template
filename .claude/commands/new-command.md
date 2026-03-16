---
description: Scaffold a new CQRS command with handler, validator using shared rules, structured logging, and Result pattern
allowed-tools: Read, Write, Edit, Glob, Grep, Bash
argument-hint: <BoundedContext> <CommandName> [ReturnType=Guid]
---

# Scaffold New CQRS Command with Best Practices

Parse `$ARGUMENTS` to extract `BoundedContext`, `CommandName`, and optional `ReturnType` (defaults to `Guid`). If missing, ask the user.

Follow ALL patterns in `CLAUDE.md`, `PROJECT_STRUCTURE.md`, and `BEST_PRACTICES.md`.

---

## Files to Create

### 1. Command Record
**Path:** `src/Application/Features/{BoundedContext}/Commands/{CommandName}/{CommandName}Command.cs`

```csharp
namespace DotnetApiDddTemplate.Application.Features.{BoundedContext}.Commands.{CommandName};

/// <summary>
/// Command to perform {action}.
/// Returns Result<{ReturnType}> - never throws exceptions for control flow.
/// </summary>
public sealed record {CommandName}Command(
    string PropertyOne,
    string PropertyTwo)
    : IRequest<Result<{ReturnType}>>;
```

---

### 2. Command Validator
**Path:** `src/Application/Features/{BoundedContext}/Commands/{CommandName}/{CommandName}CommandValidator.cs`

```csharp
namespace DotnetApiDddTemplate.Application.Features.{BoundedContext}.Commands.{CommandName};

/// <summary>
/// Validator for {CommandName} command.
/// Uses reusable SharedRules from Application/Common/Validators/SharedRules.cs
/// </summary>
public sealed class {CommandName}CommandValidator : AbstractValidator<{CommandName}Command>
{
    public {CommandName}CommandValidator()
    {
        RuleFor(x => x.PropertyOne)
            .ValidateName();

        RuleFor(x => x.PropertyTwo)
            .NotEmpty().WithMessage("PropertyTwo is required")
            .MaximumLength(500);
    }
}
```

---

### 3. Command Handler
**Path:** `src/Application/Features/{BoundedContext}/Commands/{CommandName}/{CommandName}CommandHandler.cs`

```csharp
namespace DotnetApiDddTemplate.Application.Features.{BoundedContext}.Commands.{CommandName};

/// <summary>
/// Handler for {CommandName} command.
/// Orchestrates application logic: domain validation → persistence → domain events.
///
/// Injected dependencies:
/// - IRepository<T> for data access
/// - IUnitOfWork for transaction management
/// - ICurrentUser for authorization context and auditing
/// - ILogger<T> for structured logging
/// </summary>
public sealed class {CommandName}CommandHandler(
    I{AggregateName}Repository {aggregateRepository},
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ILogger<{CommandName}CommandHandler> logger)
    : IRequestHandler<{CommandName}Command, Result<{ReturnType}>>
{
    public async Task<Result<{ReturnType}>> Handle(
        {CommandName}Command request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing {CommandName} - User: {UserId}, Properties: {PropertyOne}",
            nameof({CommandName}CommandHandler),
            currentUser.Id,
            request.PropertyOne);

        // Step 1: Domain logic via factory or aggregate method
        var result = {AggregateName}.Create(request.PropertyOne, request.PropertyTwo);

        if (result.IsFailure)
        {
            logger.LogWarning(
                "Domain validation failed for {CommandName}: {ErrorCode} - {ErrorMessage}",
                nameof({CommandName}CommandHandler),
                result.Error.Code,
                result.Error.Message);

            return Result<{ReturnType}>.Failure(result.Error);
        }

        var aggregate = result.Value;

        // Step 2: Persist to database
        await {aggregateRepository}.AddAsync(aggregate, cancellationToken);

        // Step 3: Save changes (triggers interceptors: AuditableInterceptor, DomainEventDispatcher)
        // AuditableInterceptor automatically sets CreatedBy, CreatedAtUtc
        // DomainEventDispatcherInterceptor detects and queues domain events
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency exception in {CommandName}", nameof({CommandName}CommandHandler));
            return Result<{ReturnType}>.Failure(new Error(
                "Concurrency.Conflict",
                "The record was modified by another user. Please refresh and try again."));
        }

        // Step 4: Return success with ID
        logger.LogInformation(
            "Successfully processed {CommandName} - AggregateId: {AggregateId}",
            nameof({CommandName}CommandHandler),
            aggregate.Id.Value);

        return Result<{ReturnType}>.Success(aggregate.Id.Value);
    }
}
```

---

## Integration in Controller

**Path:** `src/Api/Controllers/V1/{BoundedContext}Controller.cs` (example excerpt)

```csharp
/// <summary>
/// Creates a new {AggregateName}.
/// </summary>
/// <remarks>
/// Requires authentication.
/// Raises {AggregateName}CreatedDomainEvent which triggers:
/// - Email notifications
/// - Audit logs
/// - External integrations (via domain event handlers)
/// </remarks>
/// <param name="request">Creation request</param>
/// <param name="ct">Cancellation token</param>
/// <returns>Created resource ID</returns>
/// <response code="201">Successfully created</response>
/// <response code="400">Validation failed</response>
/// <response code="409">Concurrency conflict - resource modified by another user</response>
[HttpPost]
[Authorize]
[ProduceResponseType(typeof(Guid), StatusCodes.Status201Created)]
[ProduceResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
[ProduceResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
public async Task<IActionResult> Create(
    [FromBody] Create{AggregateName}Request request,
    CancellationToken ct)
{
    logger.LogInformation(
        "HTTP POST - Create {AggregateName}: {Name}",
        "{AggregateName}",
        request.PropertyOne);

    var command = new {CommandName}Command(request.PropertyOne, request.PropertyTwo);
    var result = await mediator.Send(command, ct);

    if (result.IsFailure)
    {
        logger.LogWarning("Command failed: {ErrorCode}", result.Error.Code);

        return result.Error.Code == "Concurrency.Conflict"
            ? Conflict(new ErrorResponse(result.Error.Code, result.Error.Message, HttpContext.TraceIdentifier))
            : BadRequest(new ErrorResponse(result.Error.Code, result.Error.Message, HttpContext.TraceIdentifier));
    }

    logger.LogInformation("Created {AggregateName}: {AggregateId}", "{AggregateName}", result.Value);

    return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
}
```

---

## Available Shared Validation Rules

Located in `Application/Common/Validators/SharedRules.cs`:

```csharp
RuleFor(x => x.Name).ValidateName();                 // Non-empty, max 200, valid chars
RuleFor(x => x.Email).ValidateEmail();               // Valid email format
RuleFor(x => x.Description).ValidateDescription();   // Min 10, max 500
RuleFor(x => x.Quantity).ValidateQuantity();         // > 0, ≤ 10000
RuleFor(x => x.Price).ValidatePrice();               // > 0, ≤ 999999.99
RuleFor(x => x.Id).ValidateId();                     // Not empty GUID
```

---

## Key Patterns Implemented

✅ **Result<T> Pattern** - No exceptions for control flow, explicit success/failure
✅ **Structured Logging** - ILogger<T> injected, all operations logged with context
✅ **Shared Validation Rules** - Reusable, consistent validation via extensions
✅ **Transaction Management** - UnitOfWork.SaveChangesAsync() with concurrency handling
✅ **Domain Logic Isolation** - Business rules in aggregate, orchestration in handler
✅ **Auditing** - CreatedBy, CreatedAtUtc auto-set by AuditableInterceptor
✅ **Domain Events** - Raised in domain, dispatched and handled asynchronously
✅ **Concurrency Control** - DbUpdateConcurrencyException caught and returned as Result
✅ **Authorization Context** - ICurrentUser provides user identity for auditing

---

## Request Processing Flow

```
1. HTTP POST /api/v1/{resource}
   ↓ Controller receives request

2. Controller.Create()
   → logger.LogInformation("Creating...")
   → Create {CommandName}Command
   → mediator.Send(command, ct)
   ↓

3. MediatR Pipeline
   → ValidationBehavior runs {CommandName}CommandValidator
   → LoggingBehavior logs start
   ↓

4. {CommandName}CommandHandler.Handle()
   → {AggregateName}.Create() - domain validation returns Result
   → If Result.IsFailure → logger.LogWarning() + return Result.Failure
   → {aggregateRepository}.AddAsync()
   → unitOfWork.SaveChangesAsync() - triggers:
      - AuditableInterceptor sets CreatedBy
      - DomainEventDispatcherInterceptor queues events
   → catch DbUpdateConcurrencyException → return conflict Result
   → logger.LogInformation("Success")
   ↓

5. MediatR Post-Handlers
   → Dispatch domain events to INotificationHandler<>
   → Handlers execute asynchronously (email, logs, external calls)
   ↓

6. Controller returns Result
   → If success: 201 Created with Location header
   → If failure: 400 Bad Request or 409 Conflict with ErrorResponse
```

---

## Guidelines

- Use **primary constructors** for handler and validator
- **Handler MUST call** `unitOfWork.SaveChangesAsync()` - commands modify state
- **Domain logic** belongs in aggregate, NOT handler - handler orchestrates only
- **Return Result<T>** for domain errors - never throw exceptions for control flow
- **Catch DbUpdateConcurrencyException** and return as Result.Failure
- **Log at appropriate levels**: Information (normal), Warning (business errors), Error (system failures)
- **File-scoped namespaces** in all files
- **Verify build**: `dotnet build && dotnet test`

---

## Next Steps

1. Create handler with ILogger, ICurrentUser, IUnitOfWork injections
2. Create validator using SharedRules for common validations
3. Add to controller with [Authorize], correlation ID context
4. Create unit tests - test Result success/failure branches
5. Create integration tests - test with database and Testcontainers

---

**Reference:**
- `BEST_PRACTICES.md` - Result Pattern, Exception Handling, Transactional Consistency, Optimistic Concurrency
- `CLAUDE.md` - Naming conventions, architecture rules
- `SharedRules.cs` - Available validation rule extensions
