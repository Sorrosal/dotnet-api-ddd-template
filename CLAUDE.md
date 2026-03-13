# DotnetApiDddTemplate — .NET 10 Web API

API REST con Domain-Driven Design, Clean Architecture, CQRS, Result Pattern y Domain Events.
Stack: .NET 10, PostgreSQL, Entity Framework Core 10, MediatR, FluentValidation, Serilog.

## Build & Run

```bash
dotnet build
dotnet run --project src/Api
dotnet test                                    # all tests
dotnet test tests/UnitTests                    # unit only
dotnet test tests/IntegrationTests             # integration only
dotnet format --verify-no-changes              # check formatting
dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/Api
docker compose up --build
```

## Solution Structure

```
src/
  Api/                                  # Presentation layer
    Controllers/V1/                     # API versioned controllers
    Middleware/                          # Global error handling, logging
    Extensions/                         # DI registration, pipeline config
    Program.cs
  Application/                          # Use cases (CQRS)
    Common/
      Behaviors/                        # MediatR pipeline (ValidationBehavior, LoggingBehavior, UnitOfWorkBehavior)
      Interfaces/                       # IRepository<T,TId>, IUnitOfWork, ICurrentUser
      Models/                           # Result<T>, Error, PagedList<T>
    Features/
      {Feature}/
        Commands/{Name}/                # {Name}Command.cs, {Name}CommandHandler.cs, {Name}CommandValidator.cs
        Queries/{Name}/                 # {Name}Query.cs, {Name}QueryHandler.cs, {Name}Response.cs
        Events/                         # Domain event handlers (INotificationHandler)
    Mappings/                           # Extension methods or profiles for DTO mapping
  Domain/                               # Enterprise business rules — ZERO dependencies
    Common/
      BaseEntity.cs                     # Base with Id, domain event collection
      AggregateRoot.cs                  # Extends BaseEntity, adds RaiseDomainEvent
      IDomainEvent.cs                   # Marker: INotification
      ValueObject.cs                    # Abstract base with structural equality
      IStronglyTypedId.cs              # interface { Guid Value; }
    {BoundedContext}/
      Entities/                         # Aggregate roots and child entities
      ValueObjects/                     # Typed IDs, domain value objects
      Events/                           # Domain events (sealed records)
      Enums/                            # Domain enumerations
      Errors/                           # Static Error constants per entity
      Repositories/                     # Repository INTERFACES only
  Infrastructure/                       # Frameworks & drivers
    Persistence/
      ApplicationDbContext.cs
      UnitOfWork.cs
      Configurations/                   # IEntityTypeConfiguration<T>
      Repositories/                     # Repository implementations
      Interceptors/                     # DomainEventDispatcherInterceptor, AuditableInterceptor
      Migrations/
    Services/                           # External service implementations
tests/
  UnitTests/Domain/  Application/       # xUnit + NSubstitute + FluentAssertions
  IntegrationTests/Api/  Infrastructure/# xUnit + Testcontainers + FluentAssertions
  Common/                               # SharedFixtures, Builders, Fakes (Bogus)
```

## Architecture Rules

- **Domain → NOTHING.** Zero project references, zero NuGet packages (except primitives).
- **Application → Domain only.**
- **Infrastructure → Application + Domain.**
- **Api → Application + Infrastructure** (Infrastructure solo para registro DI).
- NUNCA referenciar Infrastructure desde Application o Domain.
- NUNCA poner lógica de negocio en Controllers. Controllers solo: parsear request → MediatR → response.
- NUNCA lanzar excepciones para flujo de control. Usar `Result<T>` en todo el dominio y application.
- NUNCA crear modelos de dominio anémicos. Cambios de estado siempre a través de métodos del entity.
- Domain events se levantan dentro de entidades, se despachan vía EF Core interceptor antes de SaveChanges.
- Toda comunicación cross-layer usa MediatR (`IRequest<T>` / `IRequestHandler<T>`).

## Naming Conventions

| Elemento | Convención | Ejemplo |
|---|---|---|
| Solution/Projects | PascalCase con `.` | `DotnetApiDddTemplate.Domain` |
| Namespaces | Espejo de carpetas | `DotnetApiDddTemplate.Domain.Orders.Entities` |
| Clases/Records | PascalCase | `OrderItem`, `CreateOrderCommand` |
| Interfaces | Prefijo `I` | `IOrderRepository`, `IUnitOfWork` |
| Campos privados | `_camelCase` | `_items`, `_unitOfWork` |
| Params/locals | camelCase | `orderId`, `cancellationToken` |
| Métodos async | Sufijo `Async` | `GetByIdAsync`, `SaveChangesAsync` |
| Commands | `{Verbo}{Sustantivo}Command` | `CreateOrderCommand` |
| Queries | `Get{Sustantivo}[List]Query` | `GetOrderByIdQuery` |
| Handlers | `{Command\|Query}Handler` | `CreateOrderCommandHandler` |
| Validators | `{Command\|Query}Validator` | `CreateOrderCommandValidator` |
| Domain Events | `{Sustantivo}{PasadoVerbo}DomainEvent` | `OrderCreatedDomainEvent` |
| Strongly Typed IDs | `{Entity}Id` | `OrderId` |
| Tests | `{Método}_Should_{Esperado}_When_{Condición}` | `Create_Should_ReturnError_When_NameEmpty` |

## Key Patterns

### Strongly Typed ID
```csharp
public readonly record struct OrderId(Guid Value) : IStronglyTypedId;
```

### Result Pattern
```csharp
public sealed class Result<T> { bool IsSuccess; T Value; Error Error; }
// Factory: Result<T>.Success(value), Result<T>.Failure(error)
// Errors: static class OrderErrors { public static readonly Error NotFound = new("Orders.NotFound", "..."); }
```

### Aggregate Root
```csharp
public sealed class Order : AggregateRoot<OrderId>
{
    private readonly List<OrderItem> _items = [];
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    private Order() { } // EF Core
    public static Result<Order> Create(string name, ...) { /* validate, RaiseDomainEvent */ }
    public Result AddItem(ProductId productId, int quantity) { /* domain logic */ }
}
```

### CQRS Command
```csharp
public sealed record CreateOrderCommand(string Name, ...) : IRequest<Result<OrderId>>;

public sealed class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateOrderCommand, Result<OrderId>>
{
    public async Task<Result<OrderId>> Handle(CreateOrderCommand request, CancellationToken ct) { ... }
}

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(200); }
}
```

### Repository Interface (Domain layer)
```csharp
public interface IOrderRepository : IRepository<Order, OrderId>
{
    Task<Order?> GetWithItemsAsync(OrderId id, CancellationToken ct = default);
}
```

## Technology Stack

| Paquete | Versión | Uso |
|---|---|---|
| .NET | 10.0 (LTS) | Target framework |
| MediatR | 12+ | CQRS pipeline |
| FluentValidation | 11+ | Request validation |
| EF Core + Npgsql | 10.x | ORM + PostgreSQL |
| Serilog | 4+ | Structured logging |
| Asp.Versioning.Mvc | 8+ | API versioning |
| xUnit | 2.9+ | Test framework |
| FluentAssertions | 7+ | Test assertions |
| NSubstitute | 5+ | Mocking |
| Testcontainers | 4+ | Integration test infra |
| Bogus | 35+ | Test data generation |

## Critical Practices

- Todo endpoint público debe estar versionado (`[ApiVersion("1")]`, ruta `api/v{version:apiVersion}/`).
- Todo command/query DEBE tener un `FluentValidation` validator.
- Pipeline behaviors MediatR: `ValidationBehavior` → `LoggingBehavior` → `UnitOfWorkBehavior`.
- Todas las entidades son `sealed` salvo diseño explícito de herencia.
- Usar `record` para Commands, Queries, DTOs, Value Objects, Domain Events.
- Usar primary constructors en handlers y servicios.
- Colecciones expuestas: siempre `IReadOnlyList<T>` o `IReadOnlyCollection<T>`.
- `file-scoped namespaces` en todos los archivos.
- `GlobalUsings.cs` por proyecto.
- Preferir collection expressions (`[]`) sobre `new List<T>()`.
- Preferir `sealed` por defecto en todas las clases.
- No usar `#region`. No usar `this.` salvo ambigüedad.
