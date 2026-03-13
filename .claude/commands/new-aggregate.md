---
description: Scaffold a new DDD aggregate root with strongly typed ID, repository interface, EF Core configuration, and domain event
allowed-tools: Read, Write, Edit, Glob, Grep, Bash
argument-hint: <BoundedContext> <AggregateName>
---

# Scaffold New Aggregate Root

Parse `$ARGUMENTS` to extract `BoundedContext` and `AggregateName`. If missing, ask the user.

Follow ALL patterns and conventions defined in the root `CLAUDE.md`. Read it first.

## Files to Create

### 1. Strongly Typed ID
**Path:** `src/Domain/{BoundedContext}/ValueObjects/{AggregateName}Id.cs`
```csharp
public readonly record struct {AggregateName}Id(Guid Value) : IStronglyTypedId;
```

### 2. Domain Errors
**Path:** `src/Domain/{BoundedContext}/Errors/{AggregateName}Errors.cs`
- Static class with common errors: `NotFound`, `AlreadyExists`, validation errors.
- Each error is a `public static readonly Error` with code `"{BoundedContext}.{AggregateName}.{ErrorName}"`.

### 3. Domain Event
**Path:** `src/Domain/{BoundedContext}/Events/{AggregateName}CreatedDomainEvent.cs`
- `public sealed record {AggregateName}CreatedDomainEvent({AggregateName}Id Id) : IDomainEvent;`

### 4. Aggregate Root Entity
**Path:** `src/Domain/{BoundedContext}/Entities/{AggregateName}.cs`
- `public sealed class {AggregateName} : AggregateRoot<{AggregateName}Id>`
- Private parameterless constructor for EF Core.
- `public static Result<{AggregateName}> Create(...)` factory method that:
  - Validates input parameters.
  - Creates the entity.
  - Raises `{AggregateName}CreatedDomainEvent`.
  - Returns `Result<{AggregateName}>.Success(entity)`.
- Add meaningful domain properties (make them `{ get; private set; }` or `init`).
- Add domain methods for state changes returning `Result` or `Result<T>`.
- Expose collections as `IReadOnlyList<T>` backed by private `List<T>`.

### 5. Repository Interface
**Path:** `src/Domain/{BoundedContext}/Repositories/I{AggregateName}Repository.cs`
- Extends `IRepository<{AggregateName}, {AggregateName}Id>`.
- Add any aggregate-specific query methods.

### 6. EF Core Configuration
**Path:** `src/Infrastructure/Persistence/Configurations/{AggregateName}Configuration.cs`
- `IEntityTypeConfiguration<{AggregateName}>`.
- Configure `{AggregateName}Id` strongly typed ID conversion: `.HasConversion(id => id.Value, value => new {AggregateName}Id(value))`.
- Configure property constraints (max lengths, required, etc.).
- Configure relationships and navigation properties.
- Configure table name as plural PascalCase.

### 7. Repository Implementation
**Path:** `src/Infrastructure/Persistence/Repositories/{AggregateName}Repository.cs`
- Implements `I{AggregateName}Repository`.
- Inject `ApplicationDbContext` via primary constructor.
- Implement all interface methods using EF Core.

## Post-Creation Steps

8. **Add DbSet** to `ApplicationDbContext`: `public DbSet<{AggregateName}> {AggregateName}s => Set<{AggregateName}>();`
9. **Register repository** in DI container (in `src/Api/Extensions/` or `src/Infrastructure/DependencyInjection.cs`).
10. **Verify** the solution builds: `dotnet build`.
