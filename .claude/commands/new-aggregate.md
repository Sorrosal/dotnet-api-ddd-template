---
description: Scaffold a new DDD aggregate root with all best practices (soft delete, auditing, strongly typed ID, concurrency control, domain events)
allowed-tools: Read, Write, Edit, Glob, Grep, Bash
argument-hint: <BoundedContext> <AggregateName>
---

# Scaffold New Aggregate Root with Best Practices

Parse `$ARGUMENTS` to extract `BoundedContext` and `AggregateName`. If missing, ask the user.

Follow ALL patterns and conventions defined in `CLAUDE.md`, `PROJECT_STRUCTURE.md`, and `BEST_PRACTICES.md`.

---

## Files to Create

### 1. Strongly Typed ID
**Path:** `src/Domain/{BoundedContext}/ValueObjects/{AggregateName}Id.cs`

```csharp
namespace DotnetApiDddTemplate.Domain.{BoundedContext}.ValueObjects;

public readonly record struct {AggregateName}Id(Guid Value) : IStronglyTypedId;
```

---

### 2. Domain Errors
**Path:** `src/Domain/{BoundedContext}/Errors/{AggregateName}Errors.cs`

```csharp
namespace DotnetApiDddTemplate.Domain.{BoundedContext}.Errors;

public static class {AggregateName}Errors
{
    public static readonly Error NotFound = new(
        "{BoundedContext}.{AggregateName}.NotFound",
        "The {AggregateName} was not found");

    public static readonly Error AlreadyExists = new(
        "{BoundedContext}.{AggregateName}.AlreadyExists",
        "A {AggregateName} with this name already exists");

    public static readonly Error InvalidName = new(
        "{BoundedContext}.{AggregateName}.InvalidName",
        "{AggregateName} name cannot be empty or exceed 200 characters");
}
```

---

### 3. Domain Event
**Path:** `src/Domain/{BoundedContext}/Events/{AggregateName}CreatedDomainEvent.cs`

```csharp
namespace DotnetApiDddTemplate.Domain.{BoundedContext}.Events;

/// <summary>
/// Raised when a new {AggregateName} is created.
/// </summary>
public sealed record {AggregateName}CreatedDomainEvent({AggregateName}Id Id) : IDomainEvent;
```

---

### 4. Aggregate Root Entity
**Path:** `src/Domain/{BoundedContext}/Entities/{AggregateName}.cs`

```csharp
namespace DotnetApiDddTemplate.Domain.{BoundedContext}.Entities;

/// <summary>
/// Aggregate root for {AggregateName}.
/// Inherits soft delete (DeletedAt), auditing (CreatedBy, ModifiedBy),
/// and optimistic concurrency (RowVersion).
/// </summary>
public sealed class {AggregateName} : AuditableEntity<{AggregateName}Id>
{
    // Properties
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    // Navigation properties (if needed)
    // private readonly List<{ChildEntity}> _items = [];
    // public IReadOnlyList<{ChildEntity}> Items => _items.AsReadOnly();

    // EF Core required
    private {AggregateName}() { }

    /// <summary>
    /// Factory method to create a new {AggregateName}.
    /// Encapsulates all validation and domain logic.
    /// </summary>
    public static Result<{AggregateName}> Create(string name, string description)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(name))
            return Result<{AggregateName}>.Failure({AggregateName}Errors.InvalidName);

        // Create
        var aggregate = new {AggregateName}
        {
            Id = new {AggregateName}Id(Guid.NewGuid()),
            Name = name,
            Description = description
        };

        // Raise domain event
        aggregate.RaiseDomainEvent(new {AggregateName}CreatedDomainEvent(aggregate.Id));

        return Result<{AggregateName}>.Success(aggregate);
    }

    /// <summary>
    /// Example domain method that performs business logic and returns Result.
    /// </summary>
    public Result UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Result.Failure({AggregateName}Errors.InvalidName);

        Name = newName;
        return Result.Success();
    }
}
```

---

### 5. Repository Interface
**Path:** `src/Domain/{BoundedContext}/Repositories/I{AggregateName}Repository.cs`

```csharp
namespace DotnetApiDddTemplate.Domain.{BoundedContext}.Repositories;

/// <summary>
/// Repository interface for {AggregateName} aggregate.
/// Extends generic IRepository with custom queries.
/// </summary>
public interface I{AggregateName}Repository : IRepository<{AggregateName}, {AggregateName}Id>
{
    /// <summary>
    /// Finds a {AggregateName} by name (excluding soft-deleted).
    /// </summary>
    Task<{AggregateName}?> GetByNameAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Gets paginated {AggregateName}s matching a specification.
    /// </summary>
    Task<List<{AggregateName}>> GetBySpecificationAsync(
        ISpecification<{AggregateName}> specification,
        CancellationToken ct = default);
}
```

---

### 6. EF Core Configuration
**Path:** `src/Infrastructure/Persistence/Configurations/{AggregateName}Configuration.cs`

```csharp
namespace DotnetApiDddTemplate.Infrastructure.Persistence.Configurations;

public sealed class {AggregateName}Configuration : IEntityTypeConfiguration<{AggregateName}>
{
    public void Configure(EntityTypeBuilder<{AggregateName}> builder)
    {
        // Table
        builder.ToTable("{AggregateName}s");

        // Primary key with strongly typed ID
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new {AggregateName}Id(value));

        // Properties
        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired();

        // Soft delete
        builder.Property(x => x.DeletedAt);

        // Auditing
        builder.Property(x => x.CreatedBy);
        builder.Property(x => x.CreatedAtUtc)
            .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");

        builder.Property(x => x.ModifiedBy);
        builder.Property(x => x.ModifiedAtUtc);

        // Optimistic concurrency
        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // Global query filter: exclude soft-deleted
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Relationships (if applicable)
        // builder.HasMany(x => x.Items)
        //     .WithOne()
        //     .HasForeignKey("AggregateId")
        //     .OnDelete(DeleteBehavior.Cascade);

        // Indexes (if needed)
        // builder.HasIndex(x => x.Name)
        //     .IsUnique()
        //     .HasFilter("DeletedAt IS NULL");
    }
}
```

---

### 7. Repository Implementation
**Path:** `src/Infrastructure/Persistence/Repositories/{AggregateName}Repository.cs`

```csharp
namespace DotnetApiDddTemplate.Infrastructure.Persistence.Repositories;

public sealed class {AggregateName}Repository(
    ApplicationDbContext context,
    ILogger<{AggregateName}Repository> logger)
    : GenericRepository<{AggregateName}, {AggregateName}Id>(context)
{
    public async Task<{AggregateName}?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        logger.LogInformation("Fetching {AggregateName} by name: {Name}", "{AggregateName}", name);

        return await Context.{AggregateName}s
            .FirstOrDefaultAsync(x => x.Name == name, ct);
    }

    public async Task<List<{AggregateName}>> GetBySpecificationAsync(
        ISpecification<{AggregateName}> specification,
        CancellationToken ct = default)
    {
        logger.LogInformation("Fetching {AggregateName}s by specification");

        return await SpecificationEvaluator<{AggregateName}>.GetQuery(Context.{AggregateName}s, specification)
            .ToListAsync(ct);
    }
}
```

---

### 8. Domain Event Handler
**Path:** `src/Application/Features/{BoundedContext}/Events/{AggregateName}CreatedDomainEventHandler.cs`

```csharp
namespace DotnetApiDddTemplate.Application.Features.{BoundedContext}.Events;

public sealed class {AggregateName}CreatedDomainEventHandler(
    ILogger<{AggregateName}CreatedDomainEventHandler> logger)
    : INotificationHandler<{AggregateName}CreatedDomainEvent>
{
    public async Task Handle({AggregateName}CreatedDomainEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Processing {AggregateName}CreatedDomainEvent for {AggregateName}Id: {Id}",
            "{AggregateName}",
            notification.Id.Value);

        // TODO: Implement domain event handling
        // Examples:
        // - Send notification email
        // - Create audit log entry
        // - Trigger external service call
        // - Update read model for queries

        await Task.CompletedTask;

        logger.LogInformation(
            "Successfully processed {AggregateName}CreatedDomainEvent for {AggregateName}Id: {Id}",
            "{AggregateName}",
            notification.Id.Value);
    }
}
```

---

## Post-Creation Steps

1. **Add DbSet to ApplicationDbContext**
   ```csharp
   public DbSet<{AggregateName}> {AggregateName}s => Set<{AggregateName}>();
   ```

2. **Register repository in DI** (`src/Infrastructure/Extensions/DependencyInjection.cs`)
   ```csharp
   services.AddScoped<I{AggregateName}Repository, {AggregateName}Repository>();
   ```

3. **Create EF Core migration**
   ```bash
   dotnet ef migrations add Add{AggregateName}Table \
       --project src/Infrastructure \
       --startup-project src/Api
   ```

4. **Update database**
   ```bash
   dotnet ef database update --project src/Infrastructure --startup-project src/Api
   ```

5. **Create controller** in `src/Api/Controllers/V1/{BoundedContext}Controller.cs`
   - Follow REST conventions
   - Use [ApiVersion("1")] and api/v{version:apiVersion}/{controller} routes
   - Inject ISender (MediatR)
   - Inject ILogger<OrdersController>
   - Keep controllers thin: parse request → MediatR → response

6. **Create commands and queries**
   - Use `/new-command` skill for Create, Update, Delete
   - Use `/new-query` skill for Read operations
   - Commands use Result<T>, no exceptions

7. **Verify build**
   ```bash
   dotnet build
   dotnet test
   ```

---

## Key Patterns Included

✅ **Soft Delete** - DeletedAt field, global query filter
✅ **Entity Auditing** - CreatedBy, CreatedAtUtc, ModifiedBy, ModifiedAtUtc
✅ **Optimistic Concurrency** - RowVersion for concurrency control
✅ **Result Pattern** - Factory method returns Result<T>
✅ **Strongly Typed IDs** - Type-safe aggregate identifiers
✅ **Domain Events** - Raised in factory, handled asynchronously
✅ **Structured Logging** - ILogger injected with correlation context
✅ **Repository Pattern** - Specification support for complex queries

---

**Reference:** See `BEST_PRACTICES.md` for full documentation of soft delete, auditing, and concurrency patterns.
