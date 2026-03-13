---
description: Scaffold a new DDD value object with validation and Result pattern
allowed-tools: Read, Write, Edit, Glob, Grep
argument-hint: <BoundedContext> <ValueObjectName> [property:Type ...]
---

# Scaffold New Value Object

Parse `$ARGUMENTS` to extract `BoundedContext`, `ValueObjectName`, and optional property definitions (e.g. `amount:decimal currency:string`). If missing, ask the user.

Follow ALL patterns and conventions defined in the root `CLAUDE.md`. Read it first.

## File to Create

**Path:** `src/Domain/{BoundedContext}/ValueObjects/{ValueObjectName}.cs`

### Option A — Simple (use when equality is purely structural, no custom validation logic)
```csharp
public sealed record {ValueObjectName}(
    // Properties from $ARGUMENTS
);
// Add a static Create factory with validation if needed
```

### Option B — Complex (use when custom equality or complex validation is needed)
```csharp
public sealed class {ValueObjectName} : ValueObject
{
    public Type Property { get; }

    private {ValueObjectName}(Type property) { Property = property; }

    public static Result<{ValueObjectName}> Create(Type property)
    {
        // Validate inputs — return Result.Failure(error) if invalid
        return Result<{ValueObjectName}>.Success(new {ValueObjectName}(property));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Property;
        // yield return other properties...
    }
}
```

**Choose Option A** for simple data containers (e.g. `Money(decimal Amount, string Currency)`).
**Choose Option B** when you need custom equality logic or complex validation.

## EF Core Configuration

After creating the value object, update the owning entity's `IEntityTypeConfiguration<T>`:
- For owned types: `builder.OwnsOne(x => x.{PropertyName}, vo => { vo.Property(x => x.Prop).HasColumnName("..."); })`
- For strongly typed IDs (already handled via conversion): no changes needed.

## Guidelines

- Value objects are **immutable** — no setters, no mutation methods.
- Equality is based on **values**, not identity.
- Use `Create` factory pattern with validation, not constructors.
- File-scoped namespaces.
- Verify build: `dotnet build`.
