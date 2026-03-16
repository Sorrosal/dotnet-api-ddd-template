# Project Structure - DotnetApiDddTemplate

Complete documentation of project organization with DDD, Clean Architecture, CQRS, Result Pattern, and Domain Events.

**Stack:** .NET 10 LTS | PostgreSQL | EF Core 10 | MediatR | FluentValidation | ILogger (Microsoft.Extensions.Logging)

---

## 📁 Folder Tree

```
dotnet-api-ddd-template/
│
├── src/
│   ├── Api/                               # Presentation Layer (HTTP)
│   │   ├── Controllers/
│   │   │   ├── V1/                        # Versioned controllers
│   │   │   │   ├── OrdersController.cs
│   │   │   │   ├── ProductsController.cs
│   │   │   │   └── ...
│   │   │   └── BaseApiController.cs       # Base controller
│   │   ├── Middleware/
│   │   │   ├── CorrelationIdMiddleware.cs              # Request tracing
│   │   │   ├── RequestResponseLoggingMiddleware.cs     # Audit logging
│   │   │   ├── GlobalExceptionHandlerMiddleware.cs     # Exception handling
│   │   │   └── ...
│   │   ├── Extensions/
│   │   │   ├── DependencyInjection.cs                  # DI registration
│   │   │   ├── ApiVersioningExtensions.cs
│   │   │   ├── AuthorizationExtensions.cs              # JWT + Policies
│   │   │   ├── OpenApiExtensions.cs                    # Swagger
│   │   │   ├── HealthCheckExtensions.cs                # Health checks
│   │   │   └── ...
│   │   ├── Filters/
│   │   │   ├── SwaggerOperationFilter.cs
│   │   │   └── SwaggerSchemaFilter.cs
│   │   ├── Models/
│   │   │   ├── ErrorResponse.cs
│   │   │   └── ...
│   │   ├── Constants/
│   │   │   └── AuthorizationPolicies.cs
│   │   ├── Program.cs                     # Entry point, pipeline config
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── appsettings.Production.json
│   │   └── DotnetApiDddTemplate.Api.csproj
│   │
│   ├── Application/                       # Application Layer (CQRS)
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs  # Validates commands/queries
│   │   │   │   ├── LoggingBehavior.cs     # Logs execution
│   │   │   │   ├── UnitOfWorkBehavior.cs  # Handles transactions + concurrency
│   │   │   │   └── PerformanceBehavior.cs # (Optional) measures execution time
│   │   │   ├── Interfaces/
│   │   │   │   ├── IRepository.cs         # Generic repository interface
│   │   │   │   ├── IUnitOfWork.cs         # Transaction management (enhanced)
│   │   │   │   ├── ICurrentUser.cs        # Current authenticated user
│   │   │   │   ├── IRequestContext.cs     # Request context (correlation ID, etc.)
│   │   │   │   └── ...
│   │   │   ├── Models/
│   │   │   │   ├── Result.cs              # Result<T> for error handling
│   │   │   │   ├── Error.cs               # Error definition
│   │   │   │   ├── PagedList.cs           # Pagination
│   │   │   │   └── ...
│   │   │   ├── Validators/
│   │   │   │   └── SharedRules.cs         # Reusable validation rules
│   │   │   ├── Exceptions/
│   │   │   │   ├── ApplicationException.cs
│   │   │   │   ├── NotFoundException.cs
│   │   │   │   ├── ValidationException.cs
│   │   │   │   ├── UnauthorizedException.cs
│   │   │   │   └── ForbiddenException.cs
│   │   │   └── Constants/
│   │   │       └── ErrorMessages.cs       # Reusable error messages
│   │   │
│   │   ├── Features/
│   │   │   ├── Orders/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateOrder/
│   │   │   │   │   │   ├── CreateOrderCommand.cs
│   │   │   │   │   │   ├── CreateOrderCommandHandler.cs
│   │   │   │   │   │   └── CreateOrderCommandValidator.cs
│   │   │   │   │   ├── UpdateOrder/
│   │   │   │   │   │   ├── UpdateOrderCommand.cs
│   │   │   │   │   │   ├── UpdateOrderCommandHandler.cs
│   │   │   │   │   │   └── UpdateOrderCommandValidator.cs
│   │   │   │   │   ├── DeleteOrder/
│   │   │   │   │   │   ├── DeleteOrderCommand.cs
│   │   │   │   │   │   ├── DeleteOrderCommandHandler.cs
│   │   │   │   │   │   └── DeleteOrderCommandValidator.cs
│   │   │   │   │   └── ...
│   │   │   │   ├── Queries/
│   │   │   │   │   ├── GetOrderById/
│   │   │   │   │   │   ├── GetOrderByIdQuery.cs
│   │   │   │   │   │   ├── GetOrderByIdQueryHandler.cs
│   │   │   │   │   │   ├── GetOrderByIdQueryValidator.cs (if needed)
│   │   │   │   │   │   └── GetOrderByIdResponse.cs
│   │   │   │   │   ├── GetOrderList/
│   │   │   │   │   │   ├── GetOrderListQuery.cs
│   │   │   │   │   │   ├── GetOrderListQueryHandler.cs
│   │   │   │   │   │   └── GetOrderListResponse.cs
│   │   │   │   │   └── ...
│   │   │   │   ├── Specifications/           # Reusable query specifications
│   │   │   │   │   ├── ActiveOrdersSpecification.cs
│   │   │   │   │   ├── OrderByStatusSpecification.cs
│   │   │   │   │   └── ...
│   │   │   │   ├── Events/
│   │   │   │   │   ├── OrderCreatedDomainEventHandler.cs
│   │   │   │   │   ├── OrderDeletedDomainEventHandler.cs
│   │   │   │   │   └── ...
│   │   │   │   └── Dtos/
│   │   │   │       ├── OrderDto.cs
│   │   │   │       ├── OrderItemDto.cs
│   │   │   │       └── ...
│   │   │   │
│   │   │   ├── Products/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateProduct/
│   │   │   │   │   └── ...
│   │   │   │   ├── Queries/
│   │   │   │   ├── Events/
│   │   │   │   └── Dtos/
│   │   │   │
│   │   │   └── ... (other Features/BoundedContexts)
│   │   │
│   │   ├── Mappings/
│   │   │   ├── OrderMappingExtensions.cs  # Extension methods for mapping
│   │   │   ├── ProductMappingExtensions.cs
│   │   │   └── ...
│   │   │
│   │   └── DotnetApiDddTemplate.Application.csproj
│   │
│   ├── Domain/                            # Domain Layer (ZERO dependencies)
│   │   ├── Common/
│   │   │   ├── BaseEntity.cs              # Base: Id + DomainEvents + Soft Delete + RowVersion
│   │   │   ├── AuditableEntity.cs         # Base: CreatedBy, ModifiedBy, timestamps
│   │   │   ├── AggregateRoot.cs           # Base: Extends BaseEntity + RaiseDomainEvent
│   │   │   ├── IDomainEvent.cs            # Marker: extends INotification
│   │   │   ├── ValueObject.cs             # Base for Value Objects with structural equality
│   │   │   ├── IStronglyTypedId.cs        # Interface for strongly typed IDs
│   │   │   ├── Specifications/
│   │   │   │   ├── ISpecification.cs      # Specification pattern interface
│   │   │   │   └── Specification.cs       # Specification base class
│   │   │   └── GlobalUsings.cs
│   │   │
│   │   ├── Orders/                        # Bounded Context: Orders
│   │   │   ├── Entities/
│   │   │   │   ├── Order.cs               # Aggregate Root
│   │   │   │   ├── OrderItem.cs           # Entity within aggregate
│   │   │   │   └── ...
│   │   │   ├── ValueObjects/
│   │   │   │   ├── OrderId.cs             # Strongly typed ID
│   │   │   │   ├── OrderItemId.cs
│   │   │   │   ├── OrderStatus.cs         # Value Object enumeration
│   │   │   │   └── ...
│   │   │   ├── Events/
│   │   │   │   ├── OrderCreatedDomainEvent.cs
│   │   │   │   ├── OrderItemAddedDomainEvent.cs
│   │   │   │   ├── OrderDeletedDomainEvent.cs
│   │   │   │   └── ...
│   │   │   ├── Enums/
│   │   │   │   └── OrderStatusEnum.cs     # Domain enumerations
│   │   │   ├── Errors/
│   │   │   │   └── OrderErrors.cs         # Domain-specific errors
│   │   │   ├── Repositories/
│   │   │   │   └── IOrderRepository.cs    # INTERFACES only, no implementations
│   │   │   └── GlobalUsings.cs
│   │   │
│   │   ├── Products/                      # Bounded Context: Products
│   │   │   ├── Entities/
│   │   │   │   └── Product.cs
│   │   │   ├── ValueObjects/
│   │   │   │   ├── ProductId.cs
│   │   │   │   ├── Money.cs               # Value Object for price
│   │   │   │   └── ...
│   │   │   ├── Events/
│   │   │   ├── Errors/
│   │   │   │   └── ProductErrors.cs
│   │   │   ├── Repositories/
│   │   │   │   └── IProductRepository.cs
│   │   │   └── GlobalUsings.cs
│   │   │
│   │   └── ... (other Bounded Contexts)
│   │   └── DotnetApiDddTemplate.Domain.csproj
│   │
│   └── Infrastructure/                    # Infrastructure Layer
│       ├── Persistence/
│       │   ├── ApplicationDbContext.cs     # Main DbContext + global query filters
│       │   ├── UnitOfWork.cs              # IUnitOfWork implementation (enhanced)
│       │   ├── Configurations/
│       │   │   ├── OrderConfiguration.cs       # IEntityTypeConfiguration<Order>
│       │   │   ├── OrderItemConfiguration.cs
│       │   │   ├── ProductConfiguration.cs
│       │   │   └── ...
│       │   ├── Repositories/
│       │   │   ├── OrderRepository.cs          # Implements IOrderRepository
│       │   │   ├── ProductRepository.cs
│       │   │   ├── GenericRepository.cs        # Generic base with Specification support
│       │   │   └── ...
│       │   ├── Specifications/
│       │   │   └── SpecificationEvaluator.cs   # Evaluates specification queries
│       │   ├── Interceptors/
│       │   │   ├── DomainEventDispatcherInterceptor.cs  # Dispatches domain events
│       │   │   ├── AuditableInterceptor.cs               # Automatic auditing
│       │   │   ├── SoftDeleteInterceptor.cs              # Soft delete logic
│       │   │   └── ...
│       │   ├── Migrations/
│       │   │   ├── 20260315000000_InitialCreate.cs
│       │   │   ├── 20260315000001_AddOrderTable.cs
│       │   │   └── ... (generated by EF Core)
│       │   └── Seeding/
│       │       └── DataSeeder.cs          # Initial data population
│       │
│       ├── Services/
│       │   ├── CurrentUserService.cs      # ICurrentUser implementation
│       │   ├── RequestContextService.cs   # IRequestContext implementation
│       │   ├── DateTimeProvider.cs
│       │   ├── ExternalServices/
│       │   │   ├── PaymentServiceClient.cs
│       │   │   └── ...
│       │   └── ...
│       │
│       ├── Configuration/
│       │   ├── DatabaseSettings.cs
│       │   ├── JwtSettings.cs
│       │   ├── LoggingSettings.cs
│       │   └── AppSettings.cs
│       │
│       ├── Extensions/
│       │   └── DependencyInjection.cs     # Infrastructure DI registration
│       │
│       ├── appsettings.json               # Default configuration
│       ├── appsettings.Development.json
│       ├── appsettings.Production.json
│       └── DotnetApiDddTemplate.Infrastructure.csproj
│
├── tests/
│   ├── UnitTests/                         # Unit tests (no I/O)
│   │   ├── Domain/
│   │   │   ├── Orders/
│   │   │   │   └── OrderTests.cs          # Tests for domain entities
│   │   │   └── Products/
│   │   ├── Application/
│   │   │   ├── Orders/
│   │   │   │   ├── CreateOrderCommandHandlerTests.cs
│   │   │   │   ├── GetOrderByIdQueryHandlerTests.cs
│   │   │   │   └── ...
│   │   │   └── ...
│   │   ├── Common/
│   │   │   ├── ResultTests.cs
│   │   │   └── ...
│   │   └── DotnetApiDddTemplate.UnitTests.csproj
│   │
│   ├── IntegrationTests/                  # Integration tests (with real I/O, Testcontainers)
│   │   ├── Api/
│   │   │   ├── Orders/
│   │   │   │   ├── CreateOrderEndpointTests.cs
│   │   │   │   ├── GetOrderByIdEndpointTests.cs
│   │   │   │   └── ...
│   │   │   └── ...
│   │   ├── Infrastructure/
│   │   │   ├── Persistence/
│   │   │   │   ├── OrderRepositoryTests.cs
│   │   │   │   ├── SpecificationTests.cs
│   │   │   │   └── ...
│   │   │   └── ...
│   │   ├── Common/
│   │   │   ├── CustomWebApplicationFactory.cs  # WebApplicationFactory for tests
│   │   │   ├── DatabaseFixture.cs              # Testcontainers PostgreSQL
│   │   │   ├── Builders/
│   │   │   │   ├── OrderBuilder.cs
│   │   │   │   └── ...
│   │   │   ├── Fakes/
│   │   │   │   ├── FakeCurrentUserService.cs
│   │   │   │   ├── FakeRequestContext.cs
│   │   │   │   └── ...
│   │   │   └── ...
│   │   └── DotnetApiDddTemplate.IntegrationTests.csproj
│   │
│   ├── ArchitectureTests/                 # Architectural compliance tests
│   │   ├── LayerDependencyTests.cs        # Enforce layer boundaries
│   │   ├── NamingConventionTests.cs       # Verify naming conventions
│   │   ├── ProjectReferenceTests.cs       # Validate project structure
│   │   └── DotnetApiDddTemplate.ArchitectureTests.csproj
│   │
│   └── Common/                            # Shared test utilities
│       ├── SharedFixtures.cs              # Common fixtures
│       ├── TestDataBuilder.cs
│       └── ...
│
├── .claude/
│   ├── commands/                          # Code templates (MCP Skills)
│   │   ├── new-aggregate.md
│   │   ├── new-command.md
│   │   ├── new-domain-event.md
│   │   ├── new-entity.md
│   │   ├── new-integration-test.md
│   │   ├── new-module.md
│   │   ├── new-query.md
│   │   ├── new-value-object.md
│   │   └── new-unit-test.md
│   └── settings.json
│
├── docker-compose.yml                    # Local orchestration: PostgreSQL, etc.
├── .gitignore
├── CLAUDE.md                             # Architecture guide and rules
├── PROJECT_STRUCTURE.md                  # This file
├── README.md                             # (Optional) Project introduction
├── Directory.Build.props                 # Shared props between projects
├── Directory.Build.targets                # Shared targets between projects
├── global.json                           # Specific .NET version
└── DotnetApiDddTemplate.sln              # Solution file

```

---

## 📖 Documentation Files

- **CLAUDE.md** - Architecture rules and conventions
- **PROJECT_STRUCTURE.md** - Project organization and patterns (this file)
- **BEST_PRACTICES.md** - Complete implementation guide for all critical practices

---

## 🏗️ Layers and Responsibilities

### 1. **Domain Layer** (`src/Domain`)
**Responsibility:** Pure business logic, technology-independent.

- **Entities:** Models with identity (aggregate roots)
- **Value Objects:** Objects without identity, equality by value
- **Domain Events:** Events that occur in the domain
- **Repositories (INTERFACES):** Contract for persistence
- **Errors:** Static domain-specific errors
- **Enums:** Domain enumerations

**Constraints:**
- ✅ Zero external dependencies (no NuGet except primitives)
- ✅ Only domain methods
- ✅ Use of `Result<T>` for error handling
- ❌ NO exceptions for control flow
- ❌ NO anemic getter/setter methods

---

### 2. **Application Layer** (`src/Application`)
**Responsibility:** Use cases (CQRS), orchestration, validation.

- **Commands:** State changes
- **Queries:** Reads
- **Handlers:** Implement logic for each command/query
- **Validators:** FluentValidation per command/query
- **Domain Event Handlers:** React to domain events
- **Behaviors:** MediatR pipeline (validation, logging, transactions)
- **DTOs:** Transfer models

**Constraints:**
- ✅ Depends on Domain
- ✅ CQRS separated (Commands vs Queries)
- ✅ Validation in Validators, NOT in handlers
- ❌ NO business logic (goes in Domain)
- ❌ NO direct Infrastructure dependency (inject interfaces)

---

### 3. **Infrastructure Layer** (`src/Infrastructure`)
**Responsibility:** Technical implementations, persistence, external services.

- **Persistence:** DbContext, Repositories, UnitOfWork, Configurations
- **Interceptors:** EF Core interceptors (domain events, auditing)
- **Services:** Service implementations
- **Migrations:** EF Core migrations

**Constraints:**
- ✅ Implements interfaces from Application/Domain
- ✅ Database access, external APIs, etc.
- ❌ NO reference from Domain/Application

---

### 4. **Api Layer** (`src/Api`)
**Responsibility:** HTTP/REST, authentication, versioning.

- **Controllers:** Versioned endpoints, request parsing
- **Middleware:** Global error handling, logging
- **Extensions:** DI configuration, ASP.NET Core pipeline

**Constraints:**
- ✅ Controllers very thin: request → MediatR → response
- ✅ Mandatory versioning (`[ApiVersion("1")]`)
- ❌ NO business logic in controllers

---

### 5. **Tests**
**Responsibility:** Behavior validation.

- **Unit Tests:** Domain + Application (no I/O)
- **Integration Tests:** API endpoints + Persistence (real I/O, Testcontainers)
- **Common:** Shared Builders, Fakes, Fixtures

---

## 📐 Core Patterns

### Result<T> Pattern
The Result pattern replaces exceptions with explicit error handling, making control flow clear and testable.

```csharp
// Application/Common/Models/Result.cs
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error Error { get; }

    private Result(bool isSuccess, T? value, Error error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) =>
        new(true, value, Error.None);

    public static Result<T> Failure(Error error) =>
        new(false, default, error);

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value!) : onFailure(Error);

    public async Task<TResult> MatchAsync<TResult>(
        Func<T, Task<TResult>> onSuccess,
        Func<Error, Task<TResult>> onFailure) =>
        IsSuccess ? await onSuccess(Value!) : await onFailure(Error);
}

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}
```

**Usage:**
```csharp
// Domain/Orders/Errors/OrderErrors.cs
public static class OrderErrors
{
    public static readonly Error InvalidName = new(
        "Order.InvalidName",
        "Order name cannot be empty");

    public static readonly Error NotFound = new(
        "Order.NotFound",
        "The order was not found");
}

// Domain/Orders/Entities/Order.cs
public static Result<Order> Create(string name, string description)
{
    if (string.IsNullOrWhiteSpace(name))
        return Result<Order>.Failure(OrderErrors.InvalidName);

    var order = new Order { Name = name, Description = description };
    order.RaiseDomainEvent(new OrderCreatedDomainEvent(order.Id));
    return Result<Order>.Success(order);
}

// Application/Features/Orders/Commands/CreateOrder/CreateOrderCommandHandler.cs
var result = Order.Create(request.Name, request.Description);
return result.Match(
    onSuccess: order => {
        await orderRepository.AddAsync(order, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result<OrderId>.Success(order.Id);
    },
    onFailure: error => Result<OrderId>.Failure(error)
);
```

---

### Strongly Typed ID
```csharp
// Domain/Orders/ValueObjects/OrderId.cs
public readonly record struct OrderId(Guid Value) : IStronglyTypedId;

// Domain/Common/IStronglyTypedId.cs
public interface IStronglyTypedId
{
    Guid Value { get; }
}
```

**Benefits:**
- Type safety: can't pass ProductId where OrderId is expected
- Self-documenting code
- Better compiler support

---

### Aggregate Root
```csharp
// Domain/Orders/Entities/Order.cs
public sealed class Order : AggregateRoot<OrderId>
{
    private readonly List<OrderItem> _items = [];
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    private Order() { } // EF Core

    public static Result<Order> Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Order>.Failure(OrderErrors.InvalidName);

        var order = new Order
        {
            Id = new OrderId(Guid.NewGuid()),
            Name = name,
            Description = description
        };

        order.RaiseDomainEvent(new OrderCreatedDomainEvent(order.Id));
        return Result<Order>.Success(order);
    }

    public Result AddItem(ProductId productId, int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(OrderErrors.InvalidQuantity);

        var item = new OrderItem
        {
            Id = new OrderItemId(Guid.NewGuid()),
            OrderId = Id,
            ProductId = productId,
            Quantity = quantity
        };

        _items.Add(item);
        RaiseDomainEvent(new OrderItemAddedDomainEvent(Id, productId, quantity));
        return Result.Success();
    }
}

// Domain/Common/AggregateRoot.cs
public abstract class AggregateRoot<TId> : BaseEntity<TId>
    where TId : struct, IStronglyTypedId
{
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        DomainEvents.Add(domainEvent);
    }
}
```

---

### CQRS Command + Handler + Validator
```csharp
// Application/Features/Orders/Commands/CreateOrder/CreateOrderCommand.cs
public sealed record CreateOrderCommand(string Name, string Description)
    : IRequest<Result<OrderId>>;

// CreateOrderCommandValidator.cs
public sealed class CreateOrderCommandValidator
    : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
    }
}

// CreateOrderCommandHandler.cs
public sealed class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateOrderCommandHandler> logger)
    : IRequestHandler<CreateOrderCommand, Result<OrderId>>
{
    public async Task<Result<OrderId>> Handle(
        CreateOrderCommand request,
        CancellationToken ct)
    {
        logger.LogInformation("Creating order: {Name}", request.Name);

        var result = Order.Create(request.Name, request.Description);

        if (result.IsFailure)
        {
            logger.LogWarning("Failed to create order: {Error}", result.Error.Message);
            return result;
        }

        await orderRepository.AddAsync(result.Value, ct);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Order created successfully: {OrderId}", result.Value.Id);
        return Result<OrderId>.Success(result.Value.Id);
    }
}
```

---

### CQRS Query + Handler + Response
```csharp
// Application/Features/Orders/Queries/GetOrderById/GetOrderByIdQuery.cs
public sealed record GetOrderByIdQuery(Guid OrderId)
    : IRequest<Result<GetOrderByIdResponse>>;

// GetOrderByIdQueryHandler.cs
public sealed class GetOrderByIdQueryHandler(
    IOrderRepository orderRepository,
    ILogger<GetOrderByIdQueryHandler> logger)
    : IRequestHandler<GetOrderByIdQuery, Result<GetOrderByIdResponse>>
{
    public async Task<Result<GetOrderByIdResponse>> Handle(
        GetOrderByIdQuery request,
        CancellationToken ct)
    {
        logger.LogInformation("Fetching order: {OrderId}", request.OrderId);

        var order = await orderRepository.GetByIdAsync(
            new OrderId(request.OrderId), ct);

        if (order is null)
        {
            logger.LogWarning("Order not found: {OrderId}", request.OrderId);
            return Result<GetOrderByIdResponse>.Failure(OrderErrors.NotFound);
        }

        var response = new GetOrderByIdResponse(
            order.Id.Value,
            order.Name,
            order.Description);

        logger.LogInformation("Order retrieved successfully: {OrderId}", request.OrderId);
        return Result<GetOrderByIdResponse>.Success(response);
    }
}

// GetOrderByIdResponse.cs
public sealed record GetOrderByIdResponse(
    Guid Id,
    string Name,
    string Description);
```

---

### Repository Interface
```csharp
// Domain/Orders/Repositories/IOrderRepository.cs
public interface IOrderRepository : IRepository<Order, OrderId>
{
    Task<Order?> GetWithItemsAsync(OrderId id, CancellationToken ct = default);
    Task<PagedList<Order>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
}

// Application/Common/Interfaces/IRepository.cs
public interface IRepository<TEntity, in TId>
    where TEntity : BaseEntity<TId>
    where TId : struct, IStronglyTypedId
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default);
    Task<List<TEntity>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    void Update(TEntity entity);
    void Delete(TEntity entity);
}
```

---

### Domain Events
```csharp
// Domain/Orders/Events/OrderCreatedDomainEvent.cs
public sealed record OrderCreatedDomainEvent(OrderId OrderId)
    : IDomainEvent;

// Domain/Common/IDomainEvent.cs
public interface IDomainEvent : INotification
{
}

// Application/Features/Orders/Events/OrderCreatedDomainEventHandler.cs
public sealed class OrderCreatedDomainEventHandler(
    ILogger<OrderCreatedDomainEventHandler> logger)
    : INotificationHandler<OrderCreatedDomainEvent>
{
    public async Task Handle(OrderCreatedDomainEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Processing OrderCreatedDomainEvent for Order: {OrderId}",
            notification.OrderId.Value);

        // Send notification email, create audit log, etc.
        await Task.Delay(100, ct); // Simulate async work

        logger.LogInformation(
            "Successfully processed OrderCreatedDomainEvent for Order: {OrderId}",
            notification.OrderId.Value);
    }
}
```

---

### Controller with Versioning
```csharp
// Api/Controllers/V1/OrdersController.cs
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public sealed class OrdersController(
    ISender mediator,
    ILogger<OrdersController> logger) : ControllerBase
{
    [HttpPost]
    [ProduceResponseType(StatusCodes.Status201Created)]
    [ProduceResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken ct)
    {
        logger.LogInformation("Creating order: {Name}", request.Name);

        var command = new CreateOrderCommand(request.Name, request.Description);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { orderId = result.Value }, result.Value)
            : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("{orderId}")]
    [ProduceResponseType(StatusCodes.Status200OK)]
    [ProduceResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid orderId,
        CancellationToken ct)
    {
        logger.LogInformation("Fetching order: {OrderId}", orderId);

        var query = new GetOrderByIdQuery(orderId);
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound();
    }
}
```

---

### ILogger Usage Throughout
```csharp
// Program.cs - DI Registration
var builder = WebApplicationBuilder.CreateBuilder(args);

// Add logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
    // Add file logging provider if needed
});

// Api/Middleware/GlobalExceptionHandlerMiddleware.cs
public sealed class GlobalExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(new
            {
                error = "An internal error occurred",
                requestId = context.TraceIdentifier
            });
        }
    }
}

// Application/Common/Behaviors/LoggingBehavior.cs
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("Executing request: {RequestName}", requestName);

        var stopwatch = Stopwatch.StartNew();

        var response = await next();

        stopwatch.Stop();
        logger.LogInformation(
            "Request completed: {RequestName} ({ElapsedMilliseconds}ms)",
            requestName,
            stopwatch.ElapsedMilliseconds);

        return response;
    }
}
```

---

## 📦 Layer Dependencies

```
Domain (↑ from nothing)
   ↑
Application (→ Domain)
   ↑
Infrastructure (→ Application, Domain)
   ↑
Api (→ Infrastructure for DI only)

✅ ALLOWED
✅ Application uses Domain interfaces
✅ Infrastructure implements Application/Domain interfaces
✅ Api injects from Infrastructure

❌ PROHIBITED
❌ Domain → Application/Infrastructure/Api
❌ Application → Infrastructure (only interface injection)
❌ Any layer → Api
```

---

## 🗂️ Naming Conventions

| Element | Pattern | Example |
|---|---|---|
| Solution/Projects | `PascalCase` with `.` | `DotnetApiDddTemplate.Domain` |
| Namespaces | Mirror folder structure | `DotnetApiDddTemplate.Domain.Orders.Entities` |
| Classes/Records | `PascalCase` | `Order`, `CreateOrderCommand` |
| Interfaces | Prefix `I` | `IOrderRepository`, `IUnitOfWork` |
| Private fields | `_camelCase` | `_items`, `_unitOfWork` |
| Parameters/locals | `camelCase` | `orderId`, `cancellationToken` |
| Async methods | Suffix `Async` | `GetByIdAsync`, `SaveChangesAsync` |
| Commands | `{Verb}{Noun}Command` | `CreateOrderCommand`, `DeleteOrderCommand` |
| Queries | `Get{Noun}[List]Query` | `GetOrderByIdQuery`, `GetOrderListQuery` |
| Handlers | `{Command\|Query}Handler` | `CreateOrderCommandHandler`, `GetOrderByIdQueryHandler` |
| Validators | `{Command\|Query}Validator` | `CreateOrderCommandValidator` |
| Domain Events | `{Noun}{PastVerb}DomainEvent` | `OrderCreatedDomainEvent`, `OrderDeletedDomainEvent` |
| Event Handlers | `{Event}Handler` | `OrderCreatedDomainEventHandler` |
| Strongly Typed IDs | `{Entity}Id` | `OrderId`, `ProductId` |
| Value Objects | Noun | `Money`, `Address`, `OrderStatus` |
| Tests | `{Method}_Should_{Expected}_When_{Condition}` | `Create_Should_ReturnError_When_NameEmpty` |
| DTOs | `{Entity}Dto` | `OrderDto`, `ProductDto` |
| Responses | `{Query}Response` | `GetOrderByIdResponse` |

---

## 🔄 Request Execution Flow (Example: Create Order)

```
1. HTTP Client
   ↓ POST /api/v1/orders { "name": "...", "description": "..." }

2. OrdersController.Create()
   - Logger.LogInformation()
   - Parse request → CreateOrderCommand
   ↓

3. MediatR Pipeline (pre-handlers)
   - ValidationBehavior: validates with CreateOrderCommandValidator
   - LoggingBehavior: logs start with Logger.LogInformation()
   ↓

4. CreateOrderCommandHandler.Handle()
   - Logger.LogInformation("Creating order...")
   - Order.Create() → domain validation with Result<T>
   - If Result.IsFailure → Logger.LogWarning() + return failure
   - orderRepository.AddAsync()
   - unitOfWork.SaveChangesAsync()
   - Logger.LogInformation("Order created successfully")
   ↓

5. EF Core SaveChanges
   - DomainEventDispatcherInterceptor: detects OrderCreatedDomainEvent
   - AuditableInterceptor: (optional) audits changes
   ↓

6. MediatR Post-Handlers
   - Dispatches OrderCreatedDomainEvent
   - OrderCreatedDomainEventHandler reacts
   - Logger.LogInformation() in event handler
   ↓

7. MediatR Pipeline (post-handlers)
   - LoggingBehavior: logs completion
   ↓

8. OrdersController
   - Result<OrderId>.IsSuccess check
   - Return 201 Created
   ↓

9. HTTP Response
   - 201 Created with Location header
```

---

## 🛠️ Tools and Commands

```bash
# Build and Run
dotnet build
dotnet run --project src/Api

# Tests
dotnet test                                        # All
dotnet test tests/UnitTests                       # Unit only
dotnet test tests/IntegrationTests                # Integration only

# Formatting
dotnet format --verify-no-changes                 # Verify
dotnet format                                      # Apply

# EF Core Migrations
dotnet ef migrations add <MigrationName> \
    --project src/Infrastructure \
    --startup-project src/Api
dotnet ef database update --project src/Infrastructure --startup-project src/Api

# Docker
docker compose up --build                         # Start PostgreSQL, etc.
```

---

## 📋 Checklist for New Features

### Add a New Bounded Context

- [ ] Create folder in `Domain/{NewContext}`
- [ ] Add entities, value objects, events, errors
- [ ] Create repository interface in `Domain/{NewContext}/Repositories/`
- [ ] Create feature in `Application/Features/{NewContext}`
- [ ] Implement Commands, Queries, Handlers, Validators
- [ ] Create handlers for Domain Events in `Application/Features/{NewContext}/Events/`
- [ ] Implement repository in `Infrastructure/Persistence/Repositories/`
- [ ] Create EF Core configuration in `Infrastructure/Persistence/Configurations/`
- [ ] Create controller in `Api/Controllers/V1/{NewContext}Controller.cs`
- [ ] Write unit tests in `tests/UnitTests/`
- [ ] Write integration tests in `tests/IntegrationTests/`
- [ ] Create EF Core migration

### Add a New Command/Query

- [ ] Create Command/Query file in corresponding folder
- [ ] Create corresponding Handler
- [ ] Create corresponding Validator
- [ ] Create Response DTO if Query
- [ ] Update Controller if new endpoint
- [ ] Write unit tests
- [ ] Write integration tests

---

## 🔐 Security and Best Practices

1. **Validation in Layers:**
   - Domain: business rule validation
   - Application: input validation with FluentValidation
   - Api: HTTP parsing and formatting

2. **Never Expose Internals:**
   - Collections always as `IReadOnlyList<T>`
   - Private properties
   - Factory methods for creation

3. **Error Handling:**
   - Use `Result<T>` always
   - NO exceptions for control flow
   - Specific errors with code and message

4. **Logging with ILogger:**
   - Inject `ILogger<T>` in handlers, services, controllers
   - Use structured logging: `logger.LogInformation("Message: {Property}", value)`
   - Log at appropriate levels: Information, Warning, Error
   - Include TraceIdentifier in error responses for correlation

5. **API Versioning:**
   - Mandatory `[ApiVersion("1")]` in controllers
   - Routes with `api/v{version:apiVersion}/`
   - Don't break previous contracts

---

## 🔗 Critical Best Practices Implemented

This project includes comprehensive implementations of critical best practices. Detailed guides are available in **BEST_PRACTICES.md**:

### 🔴 Critical Practices
1. **Specification Pattern** - Encapsulate complex queries
2. **Correlation ID / Tracing** - Track requests across logs
3. **Soft Delete** - Logical deletion with data preservation
4. **Entity Auditing** - Who changed what and when
5. **Swagger/OpenAPI** - Interactive API documentation
6. **Authorization Policies** - Role and claim-based access control

### 🟡 Important Practices
7. **Optimistic Concurrency Control** - Prevent race conditions
8. **Exception Handling** - Structured error responses
9. **Health Checks** - Application monitoring endpoints
10. **Request/Response Logging** - Complete audit trail
11. **Configuration Management** - Secure, environment-specific setup
12. **Transactional Consistency** - Explicit transaction control

### 🟢 Additional Practices
13. **Validation Rules Reusability** - Share rules across validators
14. **Architectural Testing** - Enforce layer boundaries

See **BEST_PRACTICES.md** for complete implementation details and code examples.

---

## 📚 Documentation and References

| Document | Purpose |
|---|---|
| **CLAUDE.md** | Architecture rules and conventions |
| **PROJECT_STRUCTURE.md** | Project organization (this file) |
| **BEST_PRACTICES.md** | Complete implementation guide for all practices |

### Patterns and Concepts
- **CQRS Pattern:** Separation of Commands (writes) and Queries (reads)
- **DDD:** Domain-Driven Design, domain-focused approach
- **Clean Architecture:** Framework independence and layer isolation
- **Result Pattern:** Alternative to exceptions for error handling
- **Specification Pattern:** Reusable, composable query logic
- **Domain Events:** Domain occurrences, processed asynchronously
- **Repository Pattern:** Data access abstraction
- **Unit of Work Pattern:** Transaction management
- **ILogger:** Built-in .NET logging abstraction

---

**Last Updated:** 2026-03-16
**Version:** 3.0
**Changes:** Added all 14 best practices with complete implementations
