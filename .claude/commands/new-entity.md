---
description: Scaffold a new DDD entity (non-aggregate root) with strongly typed ID and EF Core configuration
allowed-tools: Read, Write, Edit, Glob, Grep
argument-hint: <BoundedContext> <EntityName> [ParentAggregate]
---

# Scaffold New DDD Entity (Non-Aggregate)

Parse `$ARGUMENTS` to extract `BoundedContext`, `EntityName`, and optional `ParentAggregate`. If missing, ask the user.

Follow ALL patterns and conventions defined in the root `CLAUDE.md`. Read it first.

## Files to Create

### 1. Strongly Typed ID
**Path:** `src/Domain/{BoundedContext}/ValueObjects/{EntityName}Id.cs`
```csharp
public readonly record struct {EntityName}Id(Guid Value) : IStronglyTypedId;
```

### 2. Entity Class
**Path:** `src/Domain/{BoundedContext}/Entities/{EntityName}.cs`
```csharp
public sealed class {EntityName} : BaseEntity<{EntityName}Id>
{
    private {EntityName}() { } // EF Core

    public static Result<{EntityName}> Create(...) { ... }

    // Domain methods returning Result
}
```
- Private parameterless constructor for EF Core.
- `Create` factory method returning `Result<{EntityName}>` with input validation.
- Properties are `{ get; private set; }`.

### 3. EF Core Configuration
**Path:** `src/Infrastructure/Persistence/Configurations/{EntityName}Configuration.cs`
- `IEntityTypeConfiguration<{EntityName}>`.
- Configure `{EntityName}Id` conversion.
- Configure as owned entity if it belongs to an aggregate (`builder.OwnsMany(...)` on the parent config).

## If ParentAggregate is specified

4. **Read** the parent aggregate class at `src/Domain/{BoundedContext}/Entities/{ParentAggregate}.cs`.
5. **Add** a private `List<{EntityName}>` field to the aggregate root.
6. **Expose** it as `public IReadOnlyList<{EntityName}> {EntityName}s => _{entityName}s.AsReadOnly();`
7. **Add** an `Add{EntityName}(...)` domain method on the aggregate that validates and adds the child entity.
8. **Update** the parent's EF configuration to include `builder.OwnsMany<{EntityName}>(...)` or `builder.HasMany<{EntityName}>(...)`.

## Key Rules

- Child entities do **NOT** get their own repository. Access through aggregate root only.
- File-scoped namespaces in all files.
- Verify build: `dotnet build`.
