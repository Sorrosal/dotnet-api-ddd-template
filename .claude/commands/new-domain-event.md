---
description: Scaffold a new domain event and its handler with structured logging
allowed-tools: Read, Write, Edit, Glob, Grep, Bash
argument-hint: <BoundedContext> <EventName> [FeatureName]
---

# Scaffold New Domain Event with Structured Logging

Parse `$ARGUMENTS` to extract `BoundedContext`, `EventName` (without `DomainEvent` suffix), and optional `FeatureName`. If missing, ask the user.

Follow ALL patterns in `CLAUDE.md`, `PROJECT_STRUCTURE.md`, and `BEST_PRACTICES.md`.

---

## Files to Create

### 1. Domain Event
**Path:** `src/Domain/{BoundedContext}/Events/{EventName}DomainEvent.cs`

```csharp
namespace DotnetApiDddTemplate.Domain.{BoundedContext}.Events;

/// <summary>
/// Domain event: {EventName} occurred.
/// Immutable record representing a fact that happened in the domain.
/// Includes all data needed by handlers to avoid N+1 queries.
/// </summary>
public sealed record {EventName}DomainEvent(
    {AggregateName}Id AggregateId,
    string Description,
    DateTime OccurredAtUtc)
    : IDomainEvent;
```

**Guidelines:**
- Name in past tense: `OrderCreated`, `PaymentProcessed`, `ShipmentScheduled`
- Include aggregate ID (strongly typed) so handlers know what aggregate changed
- Include relevant data the handler needs (avoid loading from DB again)
- Make it immutable - use `sealed record`
- Extends `IDomainEvent` which extends `INotification` (MediatR)

---

### 2. Domain Event Handler
**Path:** `src/Application/Features/{BoundedContext}/Events/{EventName}DomainEventHandler.cs`

```csharp
namespace DotnetApiDddTemplate.Application.Features.{BoundedContext}.Events;

/// <summary>
/// Handler for {EventName}DomainEvent.
/// Executes side effects asynchronously after domain event is raised.
///
/// Injected dependencies:
/// - ILogger<T> for structured logging
/// - Services for side effects (email, integration events, read model updates, etc.)
/// - Repositories if needed to load related data
/// </summary>
public sealed class {EventName}DomainEventHandler(
    ILogger<{EventName}DomainEventHandler> logger,
    ICurrentUser currentUser)
    : INotificationHandler<{EventName}DomainEvent>
{
    public async Task Handle(
        {EventName}DomainEvent notification,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing {EventName} - AggregateId: {AggregateId}, OccurredAt: {OccurredAt}",
            nameof({EventName}DomainEvent),
            notification.AggregateId.Value,
            notification.OccurredAtUtc);

        try
        {
            // Side effect 1: Example - send notification email
            // await emailService.SendOrderCreatedEmailAsync(notification.AggregateId, cancellationToken);

            // Side effect 2: Example - update read model for queries
            // var {aggregate} = await {aggregateRepository}.GetByIdAsync(notification.AggregateId, cancellationToken);
            // if ({aggregate} is not null)
            //     await readModelService.UpdateAsync({aggregate}, cancellationToken);

            // Side effect 3: Example - publish integration event to message bus
            // await messagePublisher.PublishAsync(new OrderCreatedIntegrationEvent(notification.AggregateId), cancellationToken);

            logger.LogInformation(
                "Successfully processed {EventName} - AggregateId: {AggregateId}",
                nameof({EventName}DomainEvent),
                notification.AggregateId.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error processing {EventName} - AggregateId: {AggregateId}",
                nameof({EventName}DomainEvent),
                notification.AggregateId.Value);

            // NOTE: Throwing here will NOT rollback the original transaction
            // Domain events are dispatched AFTER SaveChangesAsync completes
            // If side effect fails, handle with retry policy, dead letter queue, etc.
            // Don't throw unless you want to fail the entire request
        }
    }
}
```

---

## Raising the Event in Aggregate

**Location:** `src/Domain/{BoundedContext}/Entities/{AggregateName}.cs`

The event must be raised INSIDE the aggregate method:

```csharp
public static Result<{AggregateName}> Create(string name, string description)
{
    // Validate...

    var aggregate = new {AggregateName}
    {
        Id = new {AggregateName}Id(Guid.NewGuid()),
        Name = name,
        Description = description
    };

    // Raise domain event - it will be dispatched by DomainEventDispatcherInterceptor
    aggregate.RaiseDomainEvent(new {EventName}DomainEvent(
        aggregate.Id,
        description,
        DateTime.UtcNow));

    return Result<{AggregateName}>.Success(aggregate);
}
```

---

## Event Dispatching Flow

```
1. Aggregate.Create() or method
   → RaiseDomainEvent(new {EventName}DomainEvent(...))
   → Event queued in DomainEvents collection
   ↓

2. Handler calls unitOfWork.SaveChangesAsync()
   ↓

3. EF Core SaveChanges
   → DomainEventDispatcherInterceptor detects raised events
   → Stores events temporarily
   ↓

4. SaveChanges completes
   → DomainEventDispatcherInterceptor dispatches to MediatR
   ↓

5. MediatR finds all INotificationHandler<{EventName}DomainEvent>
   → Executes handlers asynchronously (in order or parallel)
   ↓

6. Handlers execute side effects
   → Email, integration events, read model updates, etc.
   ↓

7. If handler throws, it doesn't rollback original transaction
   → Original transaction already committed
   → Use retry policies, dead letter queues for failures
```

---

## Key Patterns

✅ **Immutable Records** - Events are facts, never change
✅ **Past Tense Naming** - Represents what happened, not what to do
✅ **Structured Logging** - ILogger with context (AggregateId, timestamp)
✅ **Asynchronous** - Handlers run after transaction commits
✅ **Side Effects Only** - No domain logic, only reactions
✅ **Error Handling** - Failures don't rollback original transaction
✅ **Data Completeness** - Include all data needed to avoid extra DB queries
✅ **Correlation Tracking** - CorrelationId propagates through logs

---

## Side Effect Examples

### 1. Send Email Notification
```csharp
public async Task Handle({EventName}DomainEvent notification, CancellationToken cancellationToken)
{
    logger.LogInformation("Sending email for {EventName}", nameof({EventName}DomainEvent));

    var {aggregate} = await {aggregateRepository}.GetByIdAsync(notification.AggregateId, cancellationToken);
    if ({aggregate} is null)
    {
        logger.LogWarning("Aggregate not found for email - AggregateId: {AggregateId}", notification.AggregateId.Value);
        return;
    }

    var email = new Email(
        to: "{email}",
        subject: "{EventName}",
        body: $"Aggregate {notification.AggregateId} was created");

    await emailService.SendAsync(email, cancellationToken);
}
```

### 2. Update Read Model
```csharp
public async Task Handle({EventName}DomainEvent notification, CancellationToken cancellationToken)
{
    var {aggregate} = await {aggregateRepository}.GetByIdAsync(notification.AggregateId, cancellationToken);
    if ({aggregate} is null) return;

    var readModel = new {AggregateName}ReadModel
    {
        Id = {aggregate}.Id.Value,
        Name = {aggregate}.Name,
        Description = {aggregate}.Description,
        CreatedAt = notification.OccurredAtUtc
    };

    await readModelService.UpsertAsync(readModel, cancellationToken);
}
```

### 3. Publish Integration Event
```csharp
public async Task Handle({EventName}DomainEvent notification, CancellationToken cancellationToken)
{
    var integrationEvent = new {EventName}IntegrationEvent(
        AggregateId: notification.AggregateId.Value,
        Description: notification.Description,
        OccurredAt: notification.OccurredAtUtc);

    await messagePublisher.PublishAsync(integrationEvent, cancellationToken);
}
```

---

## Guidelines

- **Domain events are facts** - represent what happened, not what to do
- **Name in past tense** - `OrderCreated`, not `CreateOrder`
- **Include aggregate ID** - handlers need to know what aggregate changed
- **Immutable records** - use `sealed record`, never mutable properties
- **Side effects in handlers** - keep aggregates pure, put reactions in handlers
- **Async safe** - handlers run AFTER transaction commits (can't rollback)
- **Structured logging** - Log at Information level for normal events
- **Include required data** - Avoid N+1 queries from handlers
- **Error handling** - Use retry policies, dead letter queues for failures
- **File-scoped namespaces** in all files
- **Verify build**: `dotnet build && dotnet test`

---

## Next Steps

1. Create domain event in Domain layer
2. Raise event in aggregate method
3. Create handler in Application layer with side effects
4. Log all side effect executions with correlation ID
5. Add retry logic if side effect can fail
6. Create integration tests for event dispatch

---

**Reference:**
- `BEST_PRACTICES.md` - Domain Events, Request/Response Logging
- `CLAUDE.md` - Naming conventions (past tense)
- Domain event handlers are discovered and executed automatically by MediatR
