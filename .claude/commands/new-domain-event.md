---
description: Scaffold a new domain event record and its application-layer handler
allowed-tools: Read, Write, Edit, Glob, Grep
argument-hint: <BoundedContext> <EventName> [FeatureName]
---

# Scaffold New Domain Event

Parse `$ARGUMENTS` to extract `BoundedContext`, `EventName` (without `DomainEvent` suffix — will be appended), and optional `FeatureName` (for handler placement). If missing, ask the user.

Follow ALL patterns and conventions defined in the root `CLAUDE.md`. Read it first.

## Files to Create

### 1. Domain Event (Domain layer)
**Path:** `src/Domain/{BoundedContext}/Events/{EventName}DomainEvent.cs`
```csharp
namespace DotnetApiDddTemplate.Domain.{BoundedContext}.Events;

public sealed record {EventName}DomainEvent(
    // Include the aggregate ID and any other relevant data
    // e.g. OrderId OrderId, string CustomerName
) : IDomainEvent;
```
- `IDomainEvent` extends `INotification` (MediatR).
- Include the aggregate's strongly typed ID.
- Include any data needed by the handler to avoid loading from the database again.

### 2. Domain Event Handler (Application layer)
**Path:** `src/Application/Features/{FeatureName}/Events/{EventName}DomainEventHandler.cs`
```csharp
namespace DotnetApiDddTemplate.Application.Features.{FeatureName}.Events;

public sealed class {EventName}DomainEventHandler(
    // Inject services: IEmailService, ILogger<...>, other repositories, etc.
) : INotificationHandler<{EventName}DomainEvent>
{
    public async Task Handle({EventName}DomainEvent notification, CancellationToken cancellationToken)
    {
        // Side effect logic: send email, trigger integration event, update read model, etc.
    }
}
```

## Raising the Event

Remind the developer: the event must be raised inside the aggregate method in `src/Domain/{BoundedContext}/Entities/`.
```csharp
// Inside the aggregate's Create or state-changing method:
RaiseDomainEvent(new {EventName}DomainEvent(Id, ...));
```
The `DomainEventDispatcherInterceptor` (EF Core interceptor) will dispatch all raised events automatically before `SaveChanges` completes.

## Guidelines

- Domain events represent **facts** — name them in past tense (`OrderCreated`, `PaymentProcessed`).
- Events are **immutable** records — no mutable state.
- Handlers are the place for side effects — keep domain entities clean.
- File-scoped namespaces in all files.
- Verify build: `dotnet build`.
