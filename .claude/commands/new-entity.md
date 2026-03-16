---
description: Scaffold a new DDD entity (non-aggregate root) with soft delete, auditing, strongly typed ID, and EF Core configuration
allowed-tools: Read, Write, Edit, Glob, Grep, Bash
argument-hint: <BoundedContext> <EntityName> [ParentAggregate]
---

# Scaffold New DDD Entity (Non-Aggregate) with Best Practices

Parse `$ARGUMENTS` to extract `BoundedContext`, `EntityName`, and optional `ParentAggregate`. If missing, ask the user.

Follow ALL patterns in `CLAUDE.md`, `PROJECT_STRUCTURE.md`, and `BEST_PRACTICES.md`.

---

## Files to Create

### 1. Strongly Typed ID
**Path:** `src/Domain/{BoundedContext}/ValueObjects/{EntityName}Id.cs`

```csharp
namespace DotnetApiDddTemplate.Domain.{BoundedContext}.ValueObjects;

public readonly record struct {EntityName}Id(Guid Value) : IStronglyTypedId;
```

---

### 2. Entity Class
**Path:** `src/Domain/{BoundedContext}/Entities/{EntityName}.cs`

```csharp
namespace DotnetApiDddTemplate.Domain.{BoundedContext}.Entities;

/// <summary>
/// Non-aggregate entity within {BoundedContext} bounded context.
/// Inherits:
/// - Soft delete (DeletedAt, IsDeleted)
/// - Auditing (CreatedBy, ModifiedBy, timestamps)
/// - Optimistic concurrency (RowVersion)
///
/// This entity MUST be accessed through its parent aggregate root only.
/// It does NOT have its own repository.
/// </summary>
public sealed class {EntityName} : AuditableEntity<{EntityName}Id>
{
    // Properties
    public Guid {ParentAggregate}Id { get; set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    // EF Core required
    private {EntityName}() { }

    /// <summary>
    /// Factory method for creating a new {EntityName}.
    /// Encapsulates validation and initialization.
    /// </summary>
    public static Result<{EntityName}> Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<{EntityName}>.Failure(new Error(
                "Entity.InvalidName",
                "{EntityName} name cannot be empty"));

        var entity = new {EntityName}
        {
            Id = new {EntityName}Id(Guid.NewGuid()),
            Name = name,
            Description = description
        };

        return Result<{EntityName}>.Success(entity);
    }

    /// <summary>
    /// Domain method that modifies entity state.
    /// All state changes go through methods, not direct property assignment.
    /// </summary>
    public Result UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Result.Failure(new Error("Entity.InvalidName", "Name cannot be empty"));

        Name = newName;
        return Result.Success();
    }
}
```

---

### 3. EF Core Configuration
**Path:** `src/Infrastructure/Persistence/Configurations/{EntityName}Configuration.cs`

```csharp
namespace DotnetApiDddTemplate.Infrastructure.Persistence.Configurations;

public sealed class {EntityName}Configuration : IEntityTypeConfiguration<{EntityName}>
{
    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        // Table
        builder.ToTable("{EntityName}s");

        // Primary key
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new {EntityName}Id(value));

        // Properties
        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        // Foreign key to parent aggregate
        builder.Property(x => x.{ParentAggregate}Id)
            .IsRequired();

        // Soft delete
        builder.Property(x => x.DeletedAt);

        // Auditing
        builder.Property(x => x.CreatedBy).IsRequired();
        builder.Property(x => x.CreatedAtUtc)
            .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");

        builder.Property(x => x.ModifiedBy);
        builder.Property(x => x.ModifiedAtUtc);

        // Optimistic concurrency
        builder.Property(x => x.RowVersion).IsRowVersion();

        // Global query filter: exclude soft-deleted
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Index for foreign key
        builder.HasIndex(x => x.{ParentAggregate}Id);
    }
}
```

---

## Update Parent Aggregate

**IF** ParentAggregate is specified, update the aggregate root:

**File:** `src/Domain/{BoundedContext}/Entities/{ParentAggregate}.cs`

```csharp
public sealed class {ParentAggregate} : AuditableEntity<{ParentAggregate}Id>
{
    // ✅ Add collection field (private)
    private readonly List<{EntityName}> _{entityNamePlural} = [];

    // ✅ Expose as read-only collection
    public IReadOnlyList<{EntityName}> {EntityNamePlural} => _{entityNamePlural}.AsReadOnly();

    // ... other properties ...

    /// <summary>
    /// Domain method to add a child entity.
    /// Encapsulates business rules for adding children.
    /// </summary>
    public Result Add{EntityName}(string name, string description)
    {
        // Validation: check if already exists, max items, etc.
        if (_{entityNamePlural}.Count >= 100)
            return Result.Failure(new Error("Entity.TooMany", "Cannot have more than 100 items"));

        // Create child
        var result = {EntityName}.Create(name, description);
        if (result.IsFailure)
            return result;

        var child = result.Value;
        child.{ParentAggregate}Id = Id.Value;

        // Add to collection
        _{entityNamePlural}.Add(child);

        // Raise event if needed
        RaiseDomainEvent(new {EntityName}AddedDomainEvent(Id, child.Id));

        return Result.Success();
    }

    /// <summary>
    /// Domain method to remove a child entity (soft delete).
    /// </summary>
    public Result Remove{EntityName}({EntityName}Id childId)
    {
        var child = _{entityNamePlural}.FirstOrDefault(x => x.Id == childId);
        if (child is null)
            return Result.Failure(new Error("Entity.NotFound", "{EntityName} not found"));

        // Soft delete (don't remove from list, EF handles via interceptor)
        child.DeletedAt = DateTime.UtcNow;

        RaiseDomainEvent(new {EntityName}RemovedDomainEvent(Id, childId));

        return Result.Success();
    }
}
```

**File:** `src/Infrastructure/Persistence/Configurations/{ParentAggregate}Configuration.cs` (Update)

```csharp
public void Configure(EntityTypeBuilder<{ParentAggregate}> builder)
{
    // ... existing configuration ...

    // ✅ Add relationship to child entities
    builder.HasMany(x => x.{EntityNamePlural})
        .WithOne() // No navigation from child back to parent (good practice)
        .HasForeignKey(x => x.{ParentAggregate}Id)
        .OnDelete(DeleteBehavior.Cascade); // Delete children when parent deleted
}
```

---

## Key Rules

✅ **Child entities inherit soft delete, auditing, concurrency control** from AuditableEntity
✅ **No repository for child entities** - access only through aggregate root
✅ **Foreign key to parent** - all children know their parent ID
✅ **Collection as read-only** - modifications only through aggregate methods
✅ **Factory method validates** - Create() returns Result<T>
✅ **Domain methods return Result** - No exceptions for control flow
✅ **Soft delete via interceptor** - Child deletion is logical, not physical
✅ **Raise domain events** - Children can trigger events through parent

---

## Access Patterns

**CORRECT:**
```csharp
// Load aggregate, then work with children
var aggregate = await repository.GetByIdAsync(aggregateId);
var result = aggregate.Add{EntityName}("name", "description");
await unitOfWork.SaveChangesAsync();
```

**INCORRECT:**
```csharp
// ❌ Don't query children directly
var children = context.{EntityNamePlural}.Where(...).ToList();

// ❌ Don't inject {EntityName}Repository (it doesn't exist)
// ❌ Don't modify children outside aggregate methods
```

---

## Guidelines

- **Child entities have no repository** - access through aggregate only
- **Strongly typed IDs** - {EntityName}Id for type safety
- **Factory method validation** - Create() returns Result<T>
- **Private collection field** - _entityNames, expose as IReadOnlyList
- **Domain methods** - Add{EntityName}(), Remove{EntityName}()
- **Soft delete** - DeletedAt field, global query filter
- **Auditing** - CreatedBy, ModifiedBy auto-set by interceptor
- **Concurrency** - RowVersion for optimistic locking
- **Cascade deletes** - When parent deleted, children deleted too
- **File-scoped namespaces** in all files
- **Verify build**: `dotnet build && dotnet test`

---

**Reference:**
- `BEST_PRACTICES.md` - Soft Delete, Entity Auditing, Optimistic Concurrency
- `CLAUDE.md` - Naming conventions
