# 🏛️ Architecture Guide

Deep dive into the Domain-Driven Design, Clean Architecture, and CQRS implementation.

---

## Table of Contents
1. [Architecture Overview](#architecture-overview)
2. [Layer Responsibilities](#layer-responsibilities)
3. [CQRS Pattern](#cqrs-pattern)
4. [Domain-Driven Design](#domain-driven-design)
5. [Result Pattern](#result-pattern)
6. [Data Flow](#data-flow)
7. [Entity Lifecycle](#entity-lifecycle)
8. [Best Practices](#best-practices)

---

## Architecture Overview

### Layered Architecture
```
┌─────────────────────────────────────────────────────────┐
│                    API LAYER                             │
│            (Controllers, DTOs, Validation)               │
│              ↓ Depends on Application                    │
├─────────────────────────────────────────────────────────┤
│                APPLICATION LAYER                         │
│    (CQRS: Commands, Queries, Handlers, Validators)      │
│              ↓ Depends on Domain                         │
├─────────────────────────────────────────────────────────┤
│                  DOMAIN LAYER                            │
│        (Entities, Value Objects, Domain Events)          │
│              ↓ NO dependencies                           │
├─────────────────────────────────────────────────────────┤
│              INFRASTRUCTURE LAYER                        │
│    (Database, Identity, DI, Persistence, Services)      │
│         ↓ Implements Application + Domain               │
└─────────────────────────────────────────────────────────┘
```

### Dependency Flow
```
API  ────→  Application  ────→  Domain
              ↓
          Infrastructure
              ↑
         (Implements)
```

**Key Rule**: Domain depends on NOTHING. Application depends only on Domain.

---

## Layer Responsibilities

### 1. Domain Layer (src/Domain)
**Pure business logic, zero external dependencies**

#### Responsibilities
- Define what the business is doing
- Aggregate roots & entities
- Value objects with business rules
- Domain events that represent state changes
- Repository interfaces (abstract the persistence)
- Business logic & validation

#### Example: Customer Aggregate
```csharp
// Domain/Customers/Entities/Customer.cs
public sealed class Customer : AuditableEntity<CustomerId>
{
    // Properties: business data
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }

    // Private constructor - only domain can create
    private Customer() { }

    // Factory method - validates before creation
    public static Result<Customer> Create(string name, string email, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Customer>.Failure(CustomerErrors.NameRequired);

        if (!email.Contains("@"))
            return Result<Customer>.Failure(CustomerErrors.InvalidEmail);

        var customer = new Customer
        {
            Id = new CustomerId(Guid.NewGuid()),
            Name = name,
            Email = email,
            PhoneNumber = phoneNumber
        };

        // Raise domain event: something important happened
        customer.RaiseDomainEvent(new CustomerCreatedDomainEvent(customer.Id));

        return Result<Customer>.Success(customer);
    }

    // Business method - enforces rules
    public Result Update(string name, string email, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(CustomerErrors.NameRequired);

        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;

        RaiseDomainEvent(new CustomerUpdatedDomainEvent(Id));

        return Result.Success();
    }

    // More domain logic...
}
```

**What Domain does NOT have:**
- ❌ No HttpContext
- ❌ No DbContext
- ❌ No ILogger
- ❌ No external service calls
- ❌ No exceptions for control flow

---

### 2. Application Layer (src/Application)
**Use cases, CQRS commands/queries, orchestration**

#### Responsibilities
- Implement use cases (commands & queries)
- Coordinate domain objects
- Validate requests
- Call domain methods
- Manage transactions via IUnitOfWork
- Dispatch domain events
- Map domain objects to DTOs

#### Command Example: Create Customer
```csharp
// Application/Features/Customers/Commands/CreateCustomer/CreateCustomerCommand.cs
public sealed record CreateCustomerCommand(
    string Name,
    string Email,
    string PhoneNumber) : IRequest<Result<Guid>>;

// Handler
public sealed class CreateCustomerCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateCustomerCommandHandler> logger)
    : IRequestHandler<CreateCustomerCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken ct)
    {
        // 1. Check business rule: email must be unique
        var existingCustomer = await customerRepository.GetByEmailAsync(request.Email, ct);
        if (existingCustomer is not null)
            return Result<Guid>.Failure(CustomerErrors.EmailAlreadyExists);

        // 2. Let domain create the aggregate
        var result = Customer.Create(request.Name, request.Email, request.PhoneNumber);
        if (result.IsFailure)
            return Result<Guid>.Failure(result.Error);

        var customer = result.Value;

        // 3. Persist to repository
        await customerRepository.AddAsync(customer, ct);

        // 4. Commit transaction & dispatch domain events
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Customer created: {CustomerId}", customer.Id);
        return Result<Guid>.Success(customer.Id.Value);
    }
}

// Validator
public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().NotEmpty();
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
    }
}
```

#### Query Example: Get Customers
```csharp
// Application/Features/Customers/Queries/GetCustomerList/GetCustomerListQuery.cs
public sealed record GetCustomerListQuery(
    string? Search = null,
    int Page = 1,
    int PageSize = 10) : IRequest<Result<PagedList<GetCustomerListItemResponse>>>;

// Handler
public sealed class GetCustomerListQueryHandler(
    ICustomerRepository customerRepository,
    ILogger<GetCustomerListQueryHandler> logger)
    : IRequestHandler<GetCustomerListQuery, Result<PagedList<GetCustomerListItemResponse>>>
{
    public async Task<Result<PagedList<GetCustomerListItemResponse>>> Handle(
        GetCustomerListQuery request, CancellationToken ct)
    {
        // Use Specification Pattern for flexible queries
        var spec = new CustomersBySearchSpecification(
            request.Search,
            request.Page,
            request.PageSize);

        var customers = await customerRepository.GetPagedAsync(spec, ct);

        var response = customers.Items
            .Select(c => new GetCustomerListItemResponse(
                c.Id.Value,
                c.Name,
                c.Email,
                c.City,
                c.Country))
            .ToList();

        return Result<PagedList<GetCustomerListItemResponse>>.Success(
            new PagedList<GetCustomerListItemResponse>(
                response,
                customers.TotalCount,
                request.Page,
                request.PageSize));
    }
}
```

**Application Layer Pattern:**
```
HTTP Request
    ↓
[API Controller]
    ↓ dispatch via MediatR
[CQRS Handler]
    ↓ coordinate
[Domain Objects]
    ↓
[Repository] → [Database]
    ↓
[Domain Event Dispatcher]
    ↓
[Event Handlers]
    ↓
HTTP Response
```

---

### 3. Infrastructure Layer (src/Infrastructure)
**Frameworks, external dependencies, persistence**

#### Responsibilities
- Database configuration (EF Core)
- Repository implementations
- User/Role management (ASP.NET Identity)
- JWT token generation
- Entity mapping (Fluent API)
- Migrations
- External service integrations

#### Repository Implementation
```csharp
// Infrastructure/Persistence/Repositories/CustomerRepository.cs
public sealed class CustomerRepository(ApplicationDbContext context)
    : Repository<Customer, CustomerId>(context),
      ICustomerRepository,
      IPagedCustomerRepository
{
    // Repository methods
    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken ct)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Email == email && !c.IsDeleted, ct);
    }

    public async Task<PagedList<Customer>> GetPagedAsync(
        CustomersBySearchSpecification spec,
        CancellationToken ct)
    {
        var query = ApplySpecification(spec);
        var totalCount = await query.CountAsync(ct);
        var items = await query.ToListAsync(ct);
        return new PagedList<Customer>(items, totalCount, spec.Page, spec.PageSize);
    }
}

// Base generic repository
public class Repository<TEntity, TId>(ApplicationDbContext context)
    : IRepository<TEntity, TId>
    where TEntity : BaseEntity<TId>
    where TId : notnull
{
    protected readonly ApplicationDbContext _context = context;

    public async Task AddAsync(TEntity entity, CancellationToken ct)
    {
        await _context.Set<TEntity>().AddAsync(entity, ct);
    }

    public async Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct)
    {
        return await _context.Set<TEntity>()
            .FirstOrDefaultAsync(e => e.Id.Equals(id), ct);
    }

    public async Task<List<TEntity>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Set<TEntity>().ToListAsync(ct);
    }

    // More methods...
}
```

#### DbContext & Interceptors
```csharp
// Infrastructure/Persistence/ApplicationDbContext.cs
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
{
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<ApplicationRefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}

// DomainEventDispatcherInterceptor - fires events AFTER successful SaveChanges
public sealed class DomainEventDispatcherInterceptor(IMediator mediator) : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        // Collect domain events from all entities
        var events = eventData.Context?
            .ChangeTracker
            .Entries<IHasDomainEvents>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList() ?? [];

        // Clear events from entities
        foreach (var entity in eventData.Context?.ChangeTracker
            .Entries<IHasDomainEvents>() ?? Enumerable.Empty<EntityEntry>())
        {
            entity.Entity.ClearDomainEvents();
        }

        // Dispatch events via MediatR
        foreach (var domainEvent in events)
        {
            await mediator.Publish(domainEvent, cancellationToken);
        }

        return result;
    }
}

// AuditableInterceptor - auto-populate audit fields
public sealed class AuditableInterceptor(ICurrentUser currentUser) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var entries = eventData.Context?.ChangeTracker.Entries() ?? [];

        foreach (var entry in entries)
        {
            if (entry.Entity is not AuditableEntity auditableEntity)
                continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    auditableEntity.CreatedBy = currentUser.Id ?? "system";
                    auditableEntity.CreatedAtUtc = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    auditableEntity.ModifiedBy = currentUser.Id ?? "system";
                    auditableEntity.ModifiedAtUtc = DateTime.UtcNow;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    auditableEntity.IsDeleted = true;
                    auditableEntity.DeletedAt = DateTime.UtcNow;
                    auditableEntity.ModifiedBy = currentUser.Id ?? "system";
                    auditableEntity.ModifiedAtUtc = DateTime.UtcNow;
                    break;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
```

---

### 4. API Layer (src/Api)
**HTTP endpoints, request/response mapping, middleware**

#### Responsibilities
- HTTP request handling
- Input validation (via Application validators)
- Route mapping
- Response formatting
- Middleware pipeline
- API versioning
- Global error handling

#### Controller Example
```csharp
// Api/Controllers/V1/CustomersController.cs
[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/customers")]
public sealed class CustomersController(
    ISender sender,
    ILogger<CustomersController> logger) : ControllerBase
{
    // POST api/v1/customers
    [HttpPost]
    [Authorize]  // Requires JWT bearer token
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustomer(
        [FromBody] CreateCustomerRequest request,
        CancellationToken ct)
    {
        var command = new CreateCustomerCommand(
            request.Name,
            request.Email,
            request.PhoneNumber);

        var result = await sender.Send(command, ct);

        if (result.IsFailure)
            return BadRequest(new ErrorResponse(result.Error.Code, result.Error.Message));

        logger.LogInformation("Customer created: {CustomerId}", result.Value);
        return CreatedAtAction(nameof(GetCustomerById), new { customerId = result.Value }, result.Value);
    }

    // GET api/v1/customers
    [HttpGet]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var query = new GetCustomerListQuery(search, page, pageSize);
        var result = await sender.Send(query, ct);

        if (result.IsFailure)
            return BadRequest(new ErrorResponse(result.Error.Code, result.Error.Message));

        return Ok(result.Value);
    }

    // More endpoints...
}
```

---

## CQRS Pattern

**Command Query Responsibility Segregation**

### Separation of Concerns

```
Commands (Write Operations)
├── CreateCustomerCommand
├── UpdateCustomerCommand
└── DeleteCustomerCommand
    ↓
    Modify state
    ↓
    Return void or Id

Queries (Read Operations)
├── GetCustomerByIdQuery
├── GetCustomerListQuery
└── GetCustomerByEmailQuery
    ↓
    Never modify state
    ↓
    Return DTOs
```

### Benefits
- ✅ Clear responsibility: commands change data, queries read data
- ✅ Optimize reads & writes separately
- ✅ Easier testing: commands and queries are testable in isolation
- ✅ Enables event sourcing: every command generates events

### Implementation via MediatR
```csharp
// MediatR dispatches based on IRequest<T> type
// IRequestHandler<TRequest, TResponse> implementation

// Command: returns Result
public sealed record CreateCustomerCommand(...) : IRequest<Result<Guid>>;
public sealed class CreateCustomerCommandHandler
    : IRequestHandler<CreateCustomerCommand, Result<Guid>> { ... }

// Query: returns Result with data
public sealed record GetCustomerByIdQuery(...) : IRequest<Result<CustomerResponse>>;
public sealed class GetCustomerByIdQueryHandler
    : IRequestHandler<GetCustomerByIdQuery, Result<CustomerResponse>> { ... }

// Dispatch from controller
var result = await sender.Send(command);  // MediatR finds handler automatically
```

---

## Domain-Driven Design

### Building Blocks

#### 1. Aggregate Roots
**Entities that own other entities, control consistency**

```csharp
public sealed class Customer : AuditableEntity<CustomerId>
{
    // Aggregate root: owns its data
    // Only accessible from outside is through repository
    // Enforces business rules internally
}
```

#### 2. Entities
**Objects with identity, can change over time**

```csharp
// Customer is an entity: it has a unique Id
// Same CustomerId = same customer, even if properties differ
public sealed class Customer : AuditableEntity<CustomerId>
{
    public CustomerId Id { get; set; }
    public string Name { get; set; }

    // Entities have identity
    // Use .Equals() and .GetHashCode() override to compare by Id
}
```

#### 3. Value Objects
**Objects without identity, immutable, defined by their properties**

```csharp
// CustomerId is a value object
public readonly record struct CustomerId(Guid Value) : IStronglyTypedId;

// Two CustomerId with same Guid = same value object
var id1 = new CustomerId(guid);
var id2 = new CustomerId(guid);
// id1.Equals(id2) == true

// Other value object examples: Money, Address, Email
public sealed class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string Country { get; }

    public Address(string street, string city, string country)
    {
        Street = street ?? throw new ArgumentNullException(nameof(street));
        City = city ?? throw new ArgumentNullException(nameof(city));
        Country = country ?? throw new ArgumentNullException(nameof(country));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return Country;
    }
}

// Two addresses with same values = equal
var addr1 = new Address("123 Main", "NYC", "USA");
var addr2 = new Address("123 Main", "NYC", "USA");
// addr1.Equals(addr2) == true
```

#### 4. Repository
**Abstraction for persistence, talks to aggregate roots**

```csharp
// Domain layer: interface only
public interface ICustomerRepository : IRepository<Customer, CustomerId>
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken ct);
}

// Infrastructure layer: implementation
public sealed class CustomerRepository : Repository<Customer, CustomerId>, ICustomerRepository
{
    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken ct)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Email == email && !c.IsDeleted, ct);
    }
}

// Application layer: use via interface
var customer = await customerRepository.GetByEmailAsync("john@example.com", ct);
```

#### 5. Domain Events
**Important things that happened in the domain**

```csharp
// Domain layer: event definition
public sealed record CustomerCreatedDomainEvent(CustomerId CustomerId) : IDomainEvent;

// Domain: raise event when something important happens
public static Result<Customer> Create(string name, string email, string phoneNumber)
{
    var customer = new Customer { /* ... */ };
    customer.RaiseDomainEvent(new CustomerCreatedDomainEvent(customer.Id));
    return Result<Customer>.Success(customer);
}

// Application: handle event
public sealed class CustomerCreatedDomainEventHandler(
    IEmailService emailService,
    ILogger<CustomerCreatedDomainEventHandler> logger)
    : INotificationHandler<CustomerCreatedDomainEvent>
{
    public async Task Handle(CustomerCreatedDomainEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Customer created: {CustomerId}", notification.CustomerId);

        // Send welcome email, trigger other workflows, etc.
        await emailService.SendWelcomeEmailAsync(notification.CustomerId, ct);
    }
}
```

### Bounded Contexts
**Divide large systems into separate domain models**

In this project:
- **Auth Context** - User registration, login, token management
- **Customers Context** - Customer data management

Each has its own entities, values, and repositories.

---

## Result Pattern

**Functional error handling without exceptions**

### Problem
```csharp
// ❌ Bad: exceptions for control flow
public Customer Create(string name, string email)
{
    if (string.IsNullOrEmpty(name))
        throw new ArgumentException("Name required");  // ❌ exception for business rule

    return new Customer(name, email);
}

// Calling code
try
{
    var customer = Create("", "email@test.com");  // ❌ must catch exception
}
catch (ArgumentException ex)
{
    // handle error
}
```

### Solution
```csharp
// ✅ Good: Result<T> pattern
public static Result<Customer> Create(string name, string email)
{
    if (string.IsNullOrEmpty(name))
        return Result<Customer>.Failure(CustomerErrors.NameRequired);  // ✅ return error

    return Result<Customer>.Success(new Customer(name, email));
}

// Calling code
var result = Create("", "email@test.com");
if (result.IsFailure)
{
    return BadRequest(result.Error);  // ✅ no exception, cleaner control flow
}

var customer = result.Value;
```

### Implementation
```csharp
public sealed record Result(bool IsSuccess, Error Error)
{
    public bool IsFailure => !IsSuccess;

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
}

public sealed record Result<T>(bool IsSuccess, T? Value, Error Error)
{
    public bool IsFailure => !IsSuccess;

    public static Result<T> Success(T value) => new(true, value, Error.None);
    public static Result<T> Failure(Error error) => new(false, default, error);

    // Implicit conversion for convenience
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}

// Define domain errors
public static class CustomerErrors
{
    public static readonly Error NotFound =
        new("Customer.NotFound", "The customer was not found");

    public static readonly Error NameRequired =
        new("Customer.NameRequired", "Customer name is required");

    public static readonly Error InvalidEmail =
        new("Customer.InvalidEmail", "Customer email is invalid");

    public static readonly Error EmailAlreadyExists =
        new("Customer.EmailAlreadyExists", "A customer with this email already exists");
}
```

### Usage Pattern
```csharp
// In handlers
public Result<Guid> Handle(CreateCustomerCommand request)
{
    // 1. Validate business rules
    var result = Customer.Create(request.Name, request.Email, request.PhoneNumber);
    if (result.IsFailure)
        return Result<Guid>.Failure(result.Error);

    // 2. Use the success value
    var customer = result.Value;

    // 3. Persist
    await repository.AddAsync(customer);
    await unitOfWork.SaveChangesAsync();

    return Result<Guid>.Success(customer.Id.Value);
}

// In controller
var result = await sender.Send(command);
if (result.IsFailure)
    return BadRequest(result.Error);

return Created(result.Value);
```

---

## Data Flow

### Complete Request Flow

```
1. HTTP Request
   POST /api/v1/customers
   {
     "name": "Acme Corp",
     "email": "contact@acme.com",
     "phoneNumber": "+1234567890"
   }
   ↓

2. API Controller (CustomersController.CreateCustomer)
   - Map HTTP request to CreateCustomerRequest
   - Dispatch: await sender.Send(CreateCustomerCommand)
   ↓

3. MediatR Pipeline
   - ValidationBehavior: run CreateCustomerCommandValidator
     └─ FluentValidation checks
   - LoggingBehavior: log command details
   - Continue to handler
   ↓

4. CQRS Handler (CreateCustomerCommandHandler.Handle)
   - Check business rules (email unique)
   - Call domain: Customer.Create()
   - Persist: repository.AddAsync()
   - Commit: unitOfWork.SaveChangesAsync()
   ↓

5. EF Core SaveChanges
   - AuditableInterceptor runs: set CreatedBy, CreatedAtUtc
   - DomainEventDispatcherInterceptor runs: collect events
   - SQL INSERT executed
   ↓

6. Domain Event Dispatch
   - CustomerCreatedDomainEvent published via MediatR
   - Event handlers execute
     └─ CustomerCreatedDomainEventHandler sends welcome email
   ↓

7. Handler Returns Result<Guid>
   - Success(customerId.Value)
   ↓

8. API Controller
   - if result.IsFailure → return BadRequest(error)
   - else → return CreatedAtAction(result.Value)
   ↓

9. HTTP Response
   201 Created
   Location: /api/v1/customers/{customerId}
```

---

## Entity Lifecycle

### States
```
Transient
  ↓
Added → Inserted
  ↓
Unchanged (loaded from DB)
  ↓
Modified → Updated
  ↓
Deleted (soft delete) → Updated with IsDeleted=true
```

### EF Core Tracking
```csharp
var customer = await repository.GetByIdAsync(id);
// Entity is now Tracked by DbContext

customer.Name = "New Name";  // DbContext detects change

await unitOfWork.SaveChangesAsync();
// EF Core compares original → modified
// Generates UPDATE SQL
// AuditableInterceptor sets ModifiedBy, ModifiedAtUtc
```

### Soft Delete
```csharp
// ❌ Hard delete (avoid): DELETE FROM Customers
// ✅ Soft delete: UPDATE Customers SET IsDeleted=true, DeletedAt=...

// When deleting
entry.State = EntityState.Modified;
auditableEntity.IsDeleted = true;
auditableEntity.DeletedAt = DateTime.UtcNow;

// When querying
var customers = await context.Customers
    .Where(c => !c.IsDeleted)  // Always filter out soft-deleted
    .ToListAsync();
```

---

## Best Practices

### ✅ DO

1. **Keep Domain Pure**
   - No HTTP context, no logger, no database calls
   - Only business logic

2. **Use Result<T> Pattern**
   - Never throw exceptions for business errors
   - Return errors explicitly

3. **Define Errors Explicitly**
   - Create error constants per entity
   - Use in domain methods & handlers

4. **Validate at Boundaries**
   - Validate HTTP requests in controllers
   - Validate commands/queries in validators
   - Validate in domain methods

5. **Use Aggregates for Consistency**
   - Aggregate root owns child entities
   - Only load aggregate root, not individual entities
   - Enforce business rules in aggregate

6. **Make Entities Immutable**
   - Entities sealed by default
   - Properties private or init-only
   - Expose methods instead of setters
   - Allow change only through domain methods

7. **Raise Domain Events**
   - Emit events when important things happen
   - Events = {Entity}{PastVerb}DomainEvent
   - Dispatch after successful SaveChanges

8. **Test at All Levels**
   - Unit: domain entities, value objects
   - Application: commands, queries, validators
   - Integration: API endpoints with real database

### ❌ DON'T

1. **Don't Call Domain Logic from Infrastructure**
   - Infrastructure should only persist & retrieve
   - Domain logic lives in Domain layer

2. **Don't Let Application Know About Infrastructure**
   - Application depends on Domain interfaces
   - Infrastructure implements them
   - Swap implementations without changing Application

3. **Don't Use Exceptions for Control Flow**
   - Use Result<T> for expected errors
   - Exceptions = unexpected, exceptional situations only

4. **Don't Expose Database Models**
   - Domain entities ≠ DTOs
   - Map to response models in handlers

5. **Don't Mix Concerns in Handlers**
   - Handler orchestrates, domain does business logic
   - Don't put validation in handlers (use validators)
   - Don't put persistence logic in handlers (use repos)

6. **Don't Make Everything Public**
   - Keep entity constructors private
   - Expose through factory methods
   - Use Result<T> for validation

7. **Don't Forget Audit Trails**
   - Always track CreatedBy, CreatedAtUtc
   - Always track ModifiedBy, ModifiedAtUtc for changes
   - Use soft delete, never hard delete

---

**Remember: Architecture is about making the right things easy and wrong things hard.**

When you follow these patterns, you'll find:
- ✅ Easy to test
- ✅ Easy to reason about
- ✅ Easy to add features
- ✅ Hard to break existing functionality
- ✅ Hard to mix concerns
