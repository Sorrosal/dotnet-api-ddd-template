# DotnetApiDddTemplate — .NET 10 Web API

API REST con Domain-Driven Design, Clean Architecture, CQRS, Result Pattern, Domain Events, y ASP.NET Identity.
Stack: .NET 10, PostgreSQL, Entity Framework Core 10, MediatR, FluentValidation, Serilog, JWT Bearer.

**Características implementadas:**
- ✅ Autenticación con ASP.NET Identity + JWT Bearer
- ✅ Refresh token rotation con revocación automática
- ✅ CQRS commands y queries con MediatR
- ✅ Domain-Driven Design con agregados y value objects
- ✅ Soft delete y auditing automático
- ✅ Domain events dispatcher con interceptor
- ✅ Result<T> pattern para manejo de errores
- ✅ Validación de entrada con FluentValidation
- ✅ Optimistic concurrency control con RowVersion
- ✅ API versionada (v1, v2...)

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
  Api/                                          # Presentation layer
    Controllers/
      V1/
        AuthController.cs                       # POST /register, /login, /refresh, /logout
        CustomersController.cs                  # GET, POST, PUT, DELETE /customers
    Requests/
      Auth/
        RegisterRequest.cs, LoginRequest.cs, RefreshTokenRequest.cs, LogoutRequest.cs
      Customers/
        CreateCustomerRequest.cs, UpdateCustomerRequest.cs
    Models/                                     # Response DTOs
      ApiResponse.cs                            # Envuelve success/error
    Services/                                   # Servicios de API (ej: CurrentUser)
      CurrentUser.cs                            # Implementa ICurrentUser
    Middleware/                                 # Global error handling, CORS, logging
      GlobalExceptionHandler.cs
    Extensions/
      ServiceCollectionExtensions.cs            # AddApi()
    GlobalUsings.cs
    Program.cs                                  # Wiring principal: AddIdentity, AddAuth, AddSwagger

  Application/                                  # Use cases (CQRS)
    Common/
      Behaviors/                                # MediatR pipeline
        ValidationBehavior.cs                   # Valida commands/queries
        LoggingBehavior.cs                      # Registra request/response
        UnitOfWorkBehavior.cs                   # SaveChanges post-command
      Interfaces/
        IRepository<T,TId>.cs                   # Interfaz genérica
        IUnitOfWork.cs                          # SaveChangesAsync()
        ICurrentUser.cs                         # UserId, IsInRole()
      Models/
        Result<T>.cs, Result.cs                 # Functional error handling
        Error.cs                                # Error code + message
        PagedList<T>.cs                         # Paginación
      Utils/                                    # Helpers estáticos
    Features/
      Auth/
        Interfaces/
          IAuthService.cs                       # RegisterAsync, LoginAsync, RefreshTokenAsync, LogoutAsync
        Models/
          AuthResponse.cs                       # Token response DTO
        Errors/
          AuthErrors.cs                         # Static Error constants
        Commands/
          Register/
            RegisterCommand.cs                  # IRequest<Result<string>>
            RegisterCommandHandler.cs
            RegisterCommandValidator.cs
          Login/
            LoginCommand.cs                     # IRequest<Result<AuthResponse>>
            LoginCommandHandler.cs
            LoginCommandValidator.cs
          RefreshToken/
            RefreshTokenCommand.cs              # IRequest<Result<AuthResponse>>
            RefreshTokenCommandHandler.cs
            RefreshTokenCommandValidator.cs
          Logout/
            LogoutCommand.cs                    # IRequest<Result>
            LogoutCommandHandler.cs
            LogoutCommandValidator.cs
      Customers/
        Queries/
          GetCustomerById/
            GetCustomerByIdQuery.cs             # IRequest<Result<CustomerDto>>
            GetCustomerByIdQueryHandler.cs
            CustomerDto.cs                      # Response DTO
            CustomerByIdSpecification.cs        # Specification pattern
          GetCustomerList/
            GetCustomerListQuery.cs
            GetCustomerListQueryHandler.cs
            CustomerListSpecification.cs
        Commands/
          CreateCustomer/
            CreateCustomerCommand.cs            # IRequest<Result<Guid>>
            CreateCustomerCommandHandler.cs
            CreateCustomerCommandValidator.cs
          UpdateCustomer/
            UpdateCustomerCommand.cs
            UpdateCustomerCommandHandler.cs
            UpdateCustomerCommandValidator.cs
          DeleteCustomer/
            DeleteCustomerCommand.cs
            DeleteCustomerCommandHandler.cs
        Events/
          CustomerCreatedDomainEventHandler.cs  # INotificationHandler<CustomerCreatedDomainEvent>
          CustomerUpdatedDomainEventHandler.cs
          CustomerDeletedDomainEventHandler.cs
        Repositories/
          ICustomerRepository.cs                # Domain interface (repository pattern)
    Extensions/
      ServiceCollectionExtensions.cs            # AddApplication() - registra behaviors, handlers, validators
    GlobalUsings.cs

  Domain/                                       # Enterprise business rules — ZERO dependencies
    Common/
      BaseEntity<TId>.cs                        # Base con Id + domain events
      AuditableEntity<TId>.cs                   # Extends BaseEntity: auditing + soft delete
      AggregateRoot<TId>.cs                     # Alias de AuditableEntity (semantic)
      IHasDomainEvents.cs                       # Marker interface
      IDomainEvent.cs                           # Extends INotification (MediatR)
      ValueObject.cs                            # Abstract base con structural equality
      IStronglyTypedId.cs                       # interface { Guid Value; }
      Interfaces/
        IAuditableEntity.cs                     # Marker para entities auditables
    Customers/
      Entities/
        Customer.cs                             # Aggregate root
                                                # public CustomerId Id
                                                # public string Name, Email
                                                # public static Result<Customer> Create(...)
                                                # public Result Update(...)
                                                # public Result Delete()
      ValueObjects/
        CustomerId.cs                           # readonly record struct, IStronglyTypedId
      Events/
        CustomerCreatedDomainEvent.cs           # sealed record with event data
        CustomerUpdatedDomainEvent.cs
        CustomerDeletedDomainEvent.cs
      Errors/
        CustomerErrors.cs                       # Static Error constants
      Repositories/
        ICustomerRepository.cs                  # Interface defining repository contract

  Infrastructure/                               # Frameworks & drivers
    Persistence/
      ApplicationDbContext.cs                   # Hereda de IdentityDbContext<ApplicationUser, IdentityRole, string>
                                                # DbSet<Customer> Customers
                                                # DbSet<ApplicationRefreshToken> RefreshTokens
      UnitOfWork.cs                             # Implementa IUnitOfWork
      Configurations/
        CustomerConfiguration.cs                # IEntityTypeConfiguration<Customer>
        ApplicationRefreshTokenConfiguration.cs # IEntityTypeConfiguration<ApplicationRefreshToken>
      Repositories/
        CustomerRepository.cs                   # ICustomerRepository implementation
        RepositoryBase<T,TId>.cs                # Base repository implementation
      Interceptors/
        DomainEventDispatcherInterceptor.cs     # Despacha domain events post-SaveChanges
        AuditableInterceptor.cs                 # Set audit fields (CreatedBy, ModifiedBy, etc.)
      Migrations/
        *_Initial.cs, *_AddIdentityAndRefreshTokens.cs, etc.
    Identity/
      ApplicationUser.cs                        # Extends IdentityUser + FirstName, LastName
      ApplicationRefreshToken.cs                # Entity para refresh tokens persistidos
      JwtOptions.cs                             # Config from appsettings.json [Jwt]
      Services/
        AuthService.cs                          # IAuthService implementation
                                                # RegisterAsync, LoginAsync, RefreshTokenAsync, LogoutAsync
                                                # GenerateJwtToken(), GenerateRefreshToken()
    Extensions/
      ServiceCollectionExtensions.cs            # AddInfrastructure()
                                                # Configura DbContext, Identity, JWT, AuthService
    GlobalUsings.cs

tests/
  UnitTests/
    Domain/
      Customers/
        CreateCustomerTests.cs
        UpdateCustomerTests.cs
        DeleteCustomerTests.cs
    Application/
      Features/
        Customers/
          CreateCustomerCommandHandlerTests.cs
          GetCustomerByIdQueryHandlerTests.cs
  IntegrationTests/
    Api/
      Customers/
        CreateCustomerEndpointTests.cs
        GetCustomerByIdEndpointTests.cs
    Infrastructure/
      Persistence/
        ApplicationDbContextTests.cs
  Common/
    SharedFixtures.cs                           # Fixtures para tests
    CustomerBuilder.cs                          # Builder para crear test data
    Fakes/
      FakeAuthService.cs
      FakeRepository.cs

docker-compose.yml                              # PostgreSQL + API
appsettings.json                                # Config: Jwt, ConnectionString, Logging
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
public readonly record struct CustomerId(Guid Value) : IStronglyTypedId;
```

### Result Pattern
```csharp
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error Error { get; }

    public static Result<T> Success(T value) => new(true, value, null!);
    public static Result<T> Failure(Error error) => new(false, default, error);
}

// Errores estáticos por agregado:
public static class CustomerErrors
{
    public static readonly Error NameRequired = new("Customer.NameRequired", "Name is required");
    public static readonly Error InvalidEmail = new("Customer.InvalidEmail", "Invalid email format");
    public static readonly Error NotFound = new("Customer.NotFound", "Customer not found");
    public static readonly Error Deleted = new("Customer.Deleted", "Customer is deleted");
}
```

### Auditable Entity con Soft Delete
```csharp
// Base domain entity con domain events
public abstract class BaseEntity<TId> : IHasDomainEvents
{
    public TId Id { get; protected set; }
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}

// Extiende con auditing y soft delete
public abstract class AuditableEntity<TId> : BaseEntity<TId>
{
    public DateTime CreatedAtUtc { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted => DeletedAt.HasValue;
    public byte[] RowVersion { get; set; } = [];  // Optimistic concurrency
}

// Agregado concreto
public sealed class Customer : AuditableEntity<CustomerId>
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    private Customer() { }  // EF Core only

    public static Result<Customer> Create(string name, string email, ...)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Customer>.Failure(CustomerErrors.NameRequired);

        var customer = new Customer { Id = new CustomerId(Guid.NewGuid()), Name = name, Email = email };
        customer.RaiseDomainEvent(new CustomerCreatedDomainEvent(customer.Id, customer.Name, customer.Email));
        return Result<Customer>.Success(customer);
    }

    public Result Update(string name, string email, ...)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(CustomerErrors.NameRequired);

        Name = name;
        Email = email;
        RaiseDomainEvent(new CustomerUpdatedDomainEvent(Id, Name, Email));
        return Result.Success();
    }

    public Result Delete()
    {
        if (IsDeleted)
            return Result.Failure(CustomerErrors.Deleted);

        DeletedAt = DateTime.UtcNow;
        RaiseDomainEvent(new CustomerDeletedDomainEvent(Id));
        return Result.Success();
    }
}
```

### Domain Events
```csharp
// Marcador de interfaz (hereda de MediatR.INotification)
public interface IDomainEvent : INotification { }

// Domain event específico
public sealed record CustomerCreatedDomainEvent(CustomerId CustomerId, string Name, string Email) : IDomainEvent;

// Handler que se dispara automáticamente
public sealed class CustomerCreatedDomainEventHandler : INotificationHandler<CustomerCreatedDomainEvent>
{
    public async Task Handle(CustomerCreatedDomainEvent notification, CancellationToken ct)
    {
        // Enviar email, registrar en audit, actualizar índices, etc.
        _logger.LogInformation("Customer created: {Name}", notification.Name);
    }
}
```

### CQRS Command con Validación
```csharp
// Command: registro de usuario
public sealed record RegisterCommand(
    string Email,
    string Password,
    string? FirstName = null,
    string? LastName = null) : IRequest<Result<string>>;

// Handler: delega a IAuthService
public sealed class RegisterCommandHandler(
    IAuthService authService,
    ILogger<RegisterCommandHandler> logger) : IRequestHandler<RegisterCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RegisterCommand request, CancellationToken ct)
    {
        var result = await authService.RegisterAsync(
            request.Email, request.Password, request.FirstName, request.LastName, ct);

        if (result.IsSuccess)
            logger.LogInformation("User registered: {Email}", request.Email);
        else
            logger.LogWarning("Registration failed: {Error}", result.Error.Code);

        return result;
    }
}

// Validator
public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8);
    }
}
```

### CQRS Query con Specification Pattern
```csharp
public sealed record GetCustomerByIdQuery(Guid CustomerId) : IRequest<Result<CustomerDto>>;

public sealed class GetCustomerByIdQueryHandler(
    IRepository<Customer, CustomerId> repository) : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken ct)
    {
        var customer = await repository.FirstOrDefaultAsync(
            new CustomerByIdSpecification(new CustomerId(request.CustomerId)), ct);

        if (customer is null)
            return Result<CustomerDto>.Failure(CustomerErrors.NotFound);

        return Result<CustomerDto>.Success(new CustomerDto(customer.Id.Value, customer.Name, customer.Email));
    }
}
```

### Authentication & JWT Bearer
```csharp
// Service interface (Application layer)
public interface IAuthService
{
    Task<Result<string>> RegisterAsync(string email, string password, string? firstName, string? lastName, CancellationToken ct);
    Task<Result<AuthResponse>> LoginAsync(string email, string password, CancellationToken ct);
    Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken, CancellationToken ct);
    Task<Result> LogoutAsync(string refreshToken, CancellationToken ct);
}

// DTO de respuesta
public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    string UserId,
    string Email);

// Implementación en Infrastructure
public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    JwtOptions jwtOptions) : IAuthService
{
    public async Task<Result<string>> RegisterAsync(string email, string password, ...)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
            return Result<string>.Failure(AuthErrors.EmailAlreadyExists);

        var user = new ApplicationUser { UserName = email, Email = email, FirstName = firstName };
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return Result<string>.Failure(AuthErrors.RegistrationFailed);

        await userManager.AddToRoleAsync(user, "User");
        return Result<string>.Success(user.Id);
    }

    public async Task<Result<AuthResponse>> LoginAsync(string email, string password, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !await userManager.CheckPasswordAsync(user, password))
            return Result<AuthResponse>.Failure(AuthErrors.InvalidCredentials);

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = GenerateJwtToken(user, roles);
        var refreshToken = GenerateRefreshToken();

        var refreshTokenEntity = new ApplicationRefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(jwtOptions.RefreshTokenExpiryDays),
            IsRevoked = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.RefreshTokens.Add(refreshTokenEntity);
        await dbContext.SaveChangesAsync(ct);

        return Result<AuthResponse>.Success(new AuthResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(jwtOptions.ExpiryMinutes),
            user.Id,
            user.Email ?? string.Empty));
    }

    private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtOptions.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### Repository Interface (Domain layer)
```csharp
public interface ICustomerRepository : IRepository<Customer, CustomerId>
{
    Task<Customer?> GetWithAuditAsync(CustomerId id, CancellationToken ct = default);
    Task<PagedList<Customer>> GetActiveCustomersAsync(ISpecification<Customer> spec, CancellationToken ct);
}
```

### Domain Event Dispatcher (Interceptor)
```csharp
// Se dispara automáticamente antes de SaveChanges
public sealed class DomainEventDispatcherInterceptor(
    IMediator mediator,
    ILogger<DomainEventDispatcherInterceptor> logger) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        // Recolecta events de todas las entidades
        var entries = eventData.Context.ChangeTracker
            .Entries()
            .Where(e => e.Entity is IHasDomainEvents)
            .ToList();

        var domainEvents = entries
            .OfType<IHasDomainEvents>()
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Limpia los events
        foreach (var entry in entries.OfType<IHasDomainEvents>())
            entry.ClearDomainEvents();

        // Guarda a BD
        var saveResult = await base.SavingChangesAsync(eventData, result, cancellationToken);

        // Despacha events después de SaveChanges (garantiza que BD está actualizada)
        foreach (var domainEvent in domainEvents)
        {
            logger.LogInformation("Dispatching domain event: {EventType}", domainEvent.GetType().Name);
            await mediator.Publish(domainEvent, cancellationToken);
        }

        return saveResult;
    }
}
```

### Auditing Automático (Interceptor)
```csharp
// Se dispara automáticamente en SaveChanges
public sealed class AuditableInterceptor(
    ICurrentUser currentUser,
    ILogger<AuditableInterceptor> logger) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        var userId = currentUser.Id ?? "system";
        var utcNow = DateTime.UtcNow;

        // Busca entidades auditable
        var entries = eventData.Context.ChangeTracker.Entries()
            .Where(e => e.Entity is AuditableEntity<dynamic>)
            .ToList();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property("CreatedBy").CurrentValue = userId;
                entry.Property("CreatedAtUtc").CurrentValue = utcNow;
                logger.LogInformation("Setting created audit: {Entity}", entry.Entity.GetType().Name);
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property("ModifiedBy").CurrentValue = userId;
                entry.Property("ModifiedAtUtc").CurrentValue = utcNow;
                logger.LogInformation("Setting modified audit: {Entity}", entry.Entity.GetType().Name);
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
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
| **ASP.NET Identity.EntityFrameworkCore** | **10.0** | **User authentication & roles** |
| **System.IdentityModel.Tokens.Jwt** | **8.9+** | **JWT token generation** |
| **Authentication.JwtBearer** | **10.0** | **JWT token validation** |
| xUnit | 2.9+ | Test framework |
| FluentAssertions | 7+ | Test assertions |
| NSubstitute | 5+ | Mocking |
| Testcontainers | 4+ | Integration test infra |
| Bogus | 35+ | Test data generation |

## Critical Practices

### Arquitectura & Layering
- Todo endpoint público debe estar versionado (`[ApiVersion("1")]`, ruta `api/v{version:apiVersion}/`).
- Domain layer: **CERO dependencias** de Infrastructure o Application.
- Nunca importar desde Infrastructure en Application o Domain.
- NUNCA crear modelos de dominio anémicos (sin lógica de negocio).
- NUNCA lanzar excepciones para flujo de control. Usar `Result<T>`.
- NUNCA meter lógica de negocio en Controllers. Solo: parsear request → MediatR → response.

### CQRS & MediatR
- Todo command/query DEBE tener un `FluentValidation` validator.
- Pipeline behaviors MediatR: `ValidationBehavior` → `LoggingBehavior` → `UnitOfWorkBehavior`.
- Commands retornan `IRequest<Result<T>>` o `IRequest<Result>`.
- Queries retornan `IRequest<Result<Dto>>` (NUNCA domain entities directas).
- Handlers usan primary constructors para inyección de dependencias.

### Domain Events
- Domain events se levantan **dentro de métodos de agregados** con `RaiseDomainEvent()`.
- Se despachan **automáticamente via `DomainEventDispatcherInterceptor`** después de `SaveChanges()`.
- Los handlers de domain events son `INotificationHandler<TEvent>` de MediatR.
- Usar para: auditing, notificaciones, actualizaciones de índices, proyecciones CQRS.
- ✅ Se garantiza que la BD está actualizada antes de despachar (transactional safety).

### Soft Delete & Auditing
- Todas las entidades heredan de `AuditableEntity<TId>` (extends `BaseEntity<TId>`).
- Soft delete: campo `DeletedAt` (null = no borrado; DateTime = borrado).
- Comprobar siempre `IsDeleted` en queries: `Where(x => !x.IsDeleted)`.
- Auditing automático: `CreatedBy`, `CreatedAtUtc`, `ModifiedBy`, `ModifiedAtUtc` (set por `AuditableInterceptor`).
- `ICurrentUser` inyectado en servicios para obtener `userId` actual.
- Eliminar lógicamente: asignar `DeletedAt = DateTime.UtcNow` y lanzar `DeletedDomainEvent`.

### Concurrency Control
- Todas las `AuditableEntity` incluyen `byte[] RowVersion` para optimistic locking.
- EF Core: `IsRowVersion()` en config automáticamente genera `xmin` de PostgreSQL.
- Siempre usar en APIs PUT/PATCH: comprobar resultado de SaveChanges y retornar 409 si conflict.

### Code Style
- Todas las entidades son `sealed` salvo diseño explícito de herencia.
- Usar `record` para Commands, Queries, DTOs, Value Objects, Domain Events.
- Usar primary constructors en handlers y servicios: `public sealed class Foo(Dependency dep)`.
- Colecciones expuestas: siempre `IReadOnlyList<T>` o `IReadOnlyCollection<T>`.
- Campos privados con `private readonly List<T> _items = [];` y exponer como propiedad.
- `file-scoped namespaces` en todos los archivos: `namespace X.Y.Z;` (sin braces).
- `GlobalUsings.cs` por proyecto (centraliza imports comunes).
- Preferir collection expressions (`[]`) sobre `new List<T>()`.
- No usar `#region`. No usar `this.` salvo ambigüedad.

### Result Pattern
- Errores definidos como `static readonly Error` en clases `{Entity}Errors`.
- `Result<T>.Success(value)` y `Result<T>.Failure(error)`.
- Siempre retornar `Result<T>`, nunca `throw` en lógica de negocio.
- En handlers/queries: mapear errores a HTTP status (404, 400, 409, etc.).

### Database & Migrations
- EF Core: `DbContext` hereda de `IdentityDbContext<ApplicationUser, IdentityRole, string>`.
- Entidades registradas en `ApplicationDbContext.OnModelCreating()` via `ApplyConfigurationsFromAssembly()`.
- Configurations: `IEntityTypeConfiguration<T>` para cada agregado/entity.
- Soft delete en queries: filtro global o manual `Where(!x.IsDeleted)`.
- Migrations: `dotnet ef migrations add Name --project src/Infrastructure --startup-project src/Api`.

## API Endpoints

### Authentication Endpoints

```bash
# 1. REGISTER - Crear nueva cuenta
POST /api/v1/auth/register
Content-Type: application/json
{
  "email": "user@example.com",
  "password": "SecurePass123",
  "firstName": "John",
  "lastName": "Doe"
}
# Response: 201 Created
# { "data": "userId" }

# 2. LOGIN - Obtener tokens
POST /api/v1/auth/login
Content-Type: application/json
{
  "email": "user@example.com",
  "password": "SecurePass123"
}
# Response: 200 OK
# {
#   "accessToken": "eyJhbGciOiJIUzI1NiIs...",
#   "refreshToken": "B64EncodedRandomBytes...",
#   "accessTokenExpiresAt": "2026-03-16T10:45:30Z",
#   "userId": "...",
#   "email": "user@example.com"
# }

# 3. REFRESH - Renovar access token
POST /api/v1/auth/refresh
Content-Type: application/json
{
  "refreshToken": "B64EncodedRandomBytes..."
}
# Response: 200 OK
# { accessToken, refreshToken, accessTokenExpiresAt, userId, email }

# 4. LOGOUT - Revocar refresh token
POST /api/v1/auth/logout
Content-Type: application/json
Authorization: Bearer {accessToken}
{
  "refreshToken": "B64EncodedRandomBytes..."
}
# Response: 204 No Content
```

### Customer Endpoints (Requieren Autenticación)

```bash
# GET - Obtener clientes con paginación
GET /api/v1/customers?pageNumber=1&pageSize=10
Authorization: Bearer {accessToken}
# Response: 200 OK
# { "data": { "items": [...], "totalCount": 50, "pageNumber": 1, "pageSize": 10 } }

# GET by ID
GET /api/v1/customers/{customerId}
Authorization: Bearer {accessToken}
# Response: 200 OK | 404 NotFound | 401 Unauthorized

# POST - Crear cliente (requiere rol Admin)
POST /api/v1/customers
Content-Type: application/json
Authorization: Bearer {accessToken}
{
  "name": "Acme Corp",
  "email": "contact@acme.com",
  "phoneNumber": "+1234567890",
  "address": "123 Main St",
  "city": "Springfield",
  "country": "USA"
}
# Response: 201 Created
# { "data": { "id": "uuid", "name": "...", "email": "..." } }

# PUT - Actualizar cliente
PUT /api/v1/customers/{customerId}
Content-Type: application/json
Authorization: Bearer {accessToken}
If-Match: {eTag} # Para optimistic concurrency
{
  "name": "Updated Name",
  "email": "new@email.com"
}
# Response: 200 OK | 404 NotFound | 409 Conflict (concurrency)

# DELETE - Soft delete
DELETE /api/v1/customers/{customerId}
Authorization: Bearer {accessToken}
# Response: 204 NoContent (soft delete)
```

## Authentication Flow

```
1. USER REGISTERS
   ├─ POST /auth/register
   ├─ AuthService.RegisterAsync()
   │  ├─ Validate email unique
   │  ├─ Create ApplicationUser via UserManager
   │  ├─ Assign "User" role
   │  └─ Return userId
   └─ Client stores userId

2. USER LOGS IN
   ├─ POST /auth/login
   ├─ AuthService.LoginAsync()
   │  ├─ Find user by email
   │  ├─ Verify password
   │  ├─ Get roles
   │  ├─ Generate JWT token (15 min expiry)
   │  │  └─ Claims: sub, email, name, roles, jti
   │  ├─ Generate refresh token (random 64 bytes, base64)
   │  ├─ Store refresh token in DB (ApplicationRefreshTokens table)
   │  └─ Return AccessToken + RefreshToken
   └─ Client stores both tokens

3. USER ACCESSES PROTECTED RESOURCE
   ├─ GET /customers
   ├─ Header: Authorization: Bearer {accessToken}
   ├─ JWT Bearer middleware validates token
   │  ├─ Verify HMAC-SHA256 signature
   │  ├─ Check issuer, audience
   │  ├─ Check expiry
   │  └─ Extract claims → HttpContext.User
   ├─ [Authorize] attribute checks User.Identity.IsAuthenticated
   └─ Handler processes request with ICurrentUser injected

4. ACCESS TOKEN EXPIRES (15 min)
   ├─ GET /customers → 401 Unauthorized
   ├─ Client calls POST /auth/refresh
   ├─ AuthService.RefreshTokenAsync()
   │  ├─ Find refresh token in DB (must not be revoked)
   │  ├─ Check expiry (7 days)
   │  ├─ Mark old token as revoked
   │  ├─ Generate new JWT + new refresh token
   │  ├─ Store new refresh token in DB
   │  └─ Return new pair
   └─ Client updates tokens

5. USER LOGS OUT
   ├─ POST /auth/logout
   ├─ AuthService.LogoutAsync()
   │  ├─ Find refresh token in DB
   │  ├─ Mark as revoked
   │  └─ Return success (idempotent)
   └─ Client discards tokens

6. REFRESH TOKEN EXPIRES (7 days) or IS REVOKED
   ├─ POST /auth/refresh → 400 Bad Request
   ├─ Client must re-authenticate
   └─ User performs step 2 again
```

## Configuration (appsettings.json)

```json
{
  "Jwt": {
    "Secret": "your-super-secret-key-min-32-chars-for-sha256",
    "Issuer": "your-app",
    "Audience": "your-app-users",
    "ExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=dotnet_api_ddd;Username=postgres;Password=postgres;Port=5432;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

**JWT Secret:** Debe ser una cadena aleatoria de al menos 32 caracteres (256 bits) para HMAC-SHA256.
**ExpiryMinutes:** Tiempo de vida del access token (recomendado 15-60 min).
**RefreshTokenExpiryDays:** Tiempo de vida del refresh token (recomendado 7-30 días).

## Database Schema

### AspNetUsers (ASP.NET Identity)
```sql
CREATE TABLE public."AspNetUsers" (
    "Id" text PRIMARY KEY,
    "UserName" varchar(256),
    "Email" varchar(256),
    "PasswordHash" text,
    "SecurityStamp" text,
    "FirstName" varchar(256),          -- Custom field
    "LastName" varchar(256),           -- Custom field
    ...
);
```

### AspNetRoles (ASP.NET Identity)
```sql
CREATE TABLE public."AspNetRoles" (
    "Id" text PRIMARY KEY,
    "Name" varchar(256),
    ...
);
-- Predefined: "Admin", "User"
```

### AspNetUserRoles (ASP.NET Identity)
```sql
CREATE TABLE public."AspNetUserRoles" (
    "UserId" text FOREIGN KEY,
    "RoleId" text FOREIGN KEY,
    PRIMARY KEY ("UserId", "RoleId")
);
```

### RefreshTokens (Custom)
```sql
CREATE TABLE public."RefreshTokens" (
    "Id" uuid PRIMARY KEY,
    "UserId" text NOT NULL FOREIGN KEY,
    "Token" varchar(512) NOT NULL UNIQUE,
    "ExpiresAtUtc" timestamp NOT NULL,
    "IsRevoked" boolean NOT NULL DEFAULT false,
    "CreatedAtUtc" timestamp NOT NULL,
    INDEX ix_RefreshTokens_UserId,
    INDEX ix_RefreshTokens_Token
);
```

### Customers (Domain - Soft Delete & Auditing)
```sql
CREATE TABLE public."Customers" (
    "Id" uuid PRIMARY KEY,
    "Name" varchar(200) NOT NULL,
    "Email" varchar(256) NOT NULL,
    "PhoneNumber" varchar(20),
    "Address" text,
    "City" varchar(100),
    "Country" varchar(100),
    "CreatedAtUtc" timestamp NOT NULL,
    "CreatedBy" text NOT NULL,
    "ModifiedAtUtc" timestamp,
    "ModifiedBy" text,
    "DeletedAt" timestamp,              -- NULL = active, DateTime = soft deleted
    "xmin" int NOT NULL,                -- Row version for optimistic concurrency
    INDEX ix_Customers_DeletedAt
);
```

## Dependency Injection (ServiceCollectionExtensions)

### src/Application/Extensions/ServiceCollectionExtensions.cs
```csharp
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    // MediatR - CQRS dispatcher
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationAssembly).Assembly));

    // Behaviors - validation, logging, unit of work
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

    // Validators
    services.AddValidatorsFromAssembly(typeof(ApplicationAssembly).Assembly);

    return services;
}
```

### src/Infrastructure/Extensions/ServiceCollectionExtensions.cs
```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // Database
    services.AddDbContext<ApplicationDbContext>((sp, options) =>
    {
        options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        options.AddInterceptors(
            sp.GetRequiredService<DomainEventDispatcherInterceptor>(),
            sp.GetRequiredService<AuditableInterceptor>());
    });

    // Interceptors (para auditing y domain events)
    services.AddScoped<DomainEventDispatcherInterceptor>();
    services.AddScoped<AuditableInterceptor>();

    // Identity
    services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    // JWT Authentication
    var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
        ?? throw new InvalidOperationException("JWT configuration not found.");
    services.AddSingleton(jwtOptions);

    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    services.AddAuthorization();

    // Repositories
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    services.AddScoped(typeof(IRepository<,>), typeof(RepositoryBase<,>));
    services.AddScoped<ICustomerRepository, CustomerRepository>();

    // Auth service
    services.AddScoped<IAuthService, AuthService>();

    return services;
}
```

### src/Api/Extensions/ServiceCollectionExtensions.cs
```csharp
public static IServiceCollection AddApi(this IServiceCollection services)
{
    // Current user context
    services.AddHttpContextAccessor();
    services.AddScoped<ICurrentUser, CurrentUser>();

    // CORS
    services.AddCors(options => options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

    // API Versioning
    services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1);
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    // Controllers
    services.AddControllers();

    // Swagger
    services.AddSwaggerGen();

    // Health checks
    services.AddHealthChecks();

    return services;
}
```

## Commit Conventions

Todos los commits deben seguir **Conventional Commits** en **inglés**:

```
feat: add new feature
fix: fix a bug
chore: maintenance task
docs: documentation
refactor: code refactoring
test: test-related changes
```

### Important Rules

- ✅ Usar Conventional Commits format
- ✅ Mensajes en inglés
- ❌ **NO agregar** footer `Co-Authored-By`
- ❌ **NO mencionar** herramientas de IA en commits
- Commits aparecen como del developer que los hace

**Ejemplo:**
```bash
git commit -m "feat: add order soft delete support"
```

No:
```bash
git commit -m "feat: add order soft delete support

Co-Authored-By: Some AI Tool <noreply@example.com>"
```

---

## Troubleshooting & FAQs

### Q: "JWT token is invalid or expired" (401 Unauthorized)
**A:**
- Verify token hasn't expired (check `AccessTokenExpiresAt` from login response)
- If expired, call `POST /auth/refresh` with refresh token
- Ensure `Authorization: Bearer {token}` header format is correct
- Check JWT secret in `appsettings.json` matches configuration

### Q: "Refresh token is invalid or expired" (400 Bad Request)
**A:**
- Refresh tokens expire after 7 days (configurable in `JwtOptions.RefreshTokenExpiryDays`)
- Token might be revoked from previous logout
- Try logging in again with credentials

### Q: "User role not in claims" (403 Forbidden)
**A:**
- Ensure user has role assigned: `await userManager.AddToRoleAsync(user, "Admin")`
- Verify `[Authorize(Roles = "Admin")]` attribute on controller
- Check JWT token includes role claims

### Q: "Entity not found" even though it exists
**A:**
- Soft delete entities are filtered out: `Where(x => !x.IsDeleted)`
- Repository/query filter may exclude deleted entities
- Check `DeletedAt` timestamp value in database

### Q: "409 Conflict" on PUT request
**A:**
- Optimistic concurrency control detected entity changed
- Another request modified the entity between GET and PUT
- Send `If-Match: {ETag}` header from GET response, not old value
- Retry: GET entity again, then PUT with new ETag

### Q: "DbUpdateConcurrencyException" in SaveChanges
**A:**
- Same as 409 Conflict above
- Check `byte[] RowVersion` in EF Core change tracker
- PostgreSQL `xmin` column must match

### Q: Domain events not firing
**A:**
- Events only dispatch after successful `SaveChanges()`
- Verify `DomainEventDispatcherInterceptor` registered in `AddInfrastructure()`
- Check handlers are registered: `services.AddMediatR(...)`
- Verify handler implements `INotificationHandler<TEvent>`

### Q: Auditing fields always null
**A:**
- `AuditableInterceptor` must be registered in `AddDbContext()`
- `ICurrentUser` service must be injected and return userId
- Check `User.Identity.IsAuthenticated` is true
- Ensure `AuditableEntity<TId>` properties are settable (not readonly)

### Q: Creating database migration fails
**A:**
```bash
# Make sure Infrastructure is set as startup project or use -startup-project flag
dotnet ef migrations add MigrationName \
  --project src/Infrastructure \
  --startup-project src/Api \
  --context ApplicationDbContext

# If still fails, check DbContext is properly configured
# Verify all entity configs are in Configurations/ folder
```

### Q: Test with InMemory database vs PostgreSQL
**A:**
- Never use InMemory for production tests (soft deletes, concurrency don't work properly)
- Always use real PostgreSQL with Testcontainers in integration tests
- See [new-integration-test](../skills.md) skill for examples

### Q: "No service for type 'IAuthService' has been registered"
**A:**
- Verify `services.AddScoped<IAuthService, AuthService>();` in `ServiceCollectionExtensions.cs`
- Ensure `AddInfrastructure()` is called in `Program.cs`: `builder.Services.AddInfrastructure(config)`

### Q: CORS errors in browser
**A:**
- API has CORS enabled for `AllowAnyOrigin()` in `AddCors()`
- Client must send correct `Content-Type: application/json`
- Preflight OPTIONS request must be allowed (automatically handled)

### Q: PostgreSQL connection timeout
**A:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=dotnet_api_ddd;Username=postgres;Password=postgres;Port=5432;Timeout=30;"
  }
}
```
- Check `docker compose up` started PostgreSQL: `docker ps`
- Verify port 5432 not in use: `netstat -ano | findstr :5432` (Windows)
- Connection string matches `docker-compose.yml` services

---

## Common Patterns

### Creating a new Aggregate Root
```csharp
// 1. Create value object ID
public readonly record struct OrderId(Guid Value) : IStronglyTypedId;

// 2. Create domain events
public sealed record OrderCreatedDomainEvent(OrderId OrderId, string CustomerName) : IDomainEvent;

// 3. Create aggregate root
public sealed class Order : AuditableEntity<OrderId>
{
    public string CustomerName { get; private set; } = string.Empty;
    private readonly List<OrderItem> _items = [];
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    private Order() { }  // EF Core

    public static Result<Order> Create(string customerName, ...)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            return Result<Order>.Failure(OrderErrors.CustomerNameRequired);

        var order = new Order { Id = new OrderId(Guid.NewGuid()), CustomerName = customerName };
        order.RaiseDomainEvent(new OrderCreatedDomainEvent(order.Id, order.CustomerName));
        return Result<Order>.Success(order);
    }
}

// 4. Create repository interface in Domain
public interface IOrderRepository : IRepository<Order, OrderId>
{
    Task<Order?> GetWithItemsAsync(OrderId id, CancellationToken ct);
}

// 5. Create repository implementation in Infrastructure
public sealed class OrderRepository(ApplicationDbContext dbContext) : RepositoryBase<Order, OrderId>(dbContext), IOrderRepository
{
    public async Task<Order?> GetWithItemsAsync(OrderId id, CancellationToken ct)
    {
        return await DbSet
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted, ct);
    }
}

// 6. Create entity configuration in Infrastructure
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();
        builder.Property(o => o.CustomerName).HasMaxLength(200).IsRequired();
        builder.HasMany(o => o.Items).WithOne().OnDelete(DeleteBehavior.Cascade);
        builder.Property(o => o.RowVersion).IsRowVersion();
    }
}

// 7. Register in Infrastructure DI
services.AddScoped<IOrderRepository, OrderRepository>();

// 8. Create CQRS command in Application
public sealed record CreateOrderCommand(string CustomerName) : IRequest<Result<OrderId>>;

public sealed class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateOrderCommandHandler> logger) : IRequestHandler<CreateOrderCommand, Result<OrderId>>
{
    public async Task<Result<OrderId>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var createResult = Order.Create(request.CustomerName);
        if (!createResult.IsSuccess)
            return Result<OrderId>.Failure(createResult.Error);

        var order = createResult.Value;
        orderRepository.Add(order);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Order created: {OrderId}", order.Id);
        return Result<OrderId>.Success(order.Id);
    }
}

// 9. Create validator
public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
    }
}

// 10. Create endpoint in API
[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/orders")]
public sealed class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateOrder(
        CreateOrderRequest request,
        CancellationToken ct)
    {
        var command = new CreateOrderCommand(request.CustomerName);
        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetOrder), new { id = result.Value }, result.Value);
    }
}
```

### Adding Soft Delete to Existing Aggregate
```csharp
// Change inheritance from BaseEntity to AuditableEntity
public sealed class Product : AuditableEntity<ProductId>  // Was: BaseEntity<ProductId>
{
    // Now automatically has:
    // - CreatedAtUtc, CreatedBy
    // - ModifiedAtUtc, ModifiedBy
    // - DeletedAt (null = active, DateTime = soft deleted)
    // - IsDeleted property
    // - RowVersion for optimistic locking

    // Add delete method
    public Result Delete()
    {
        if (IsDeleted)
            return Result.Failure(ProductErrors.AlreadyDeleted);

        DeletedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ProductDeletedDomainEvent(Id));
        return Result.Success();
    }
}

// In repository queries, always filter
public async Task<Product?> GetActiveByIdAsync(ProductId id, CancellationToken ct)
{
    return await DbSet
        .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
}

// In API, map delete command to domain method
var deleteResult = product.Delete();  // Sets DeletedAt, raises event
if (!deleteResult.IsSuccess)
    return BadRequest(deleteResult.Error);

await unitOfWork.SaveChangesAsync();  // Persists + dispatches events
```

### Creating Domain Event Handler
```csharp
// 1. Define domain event (in Domain layer)
public sealed record OrderCreatedDomainEvent(OrderId OrderId, string CustomerName) : IDomainEvent;

// 2. Create handler (in Application layer)
public sealed class OrderCreatedDomainEventHandler : INotificationHandler<OrderCreatedDomainEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderCreatedDomainEventHandler> _logger;

    public OrderCreatedDomainEventHandler(IEmailService emailService, ILogger<OrderCreatedDomainEventHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Order created: {OrderId}, Customer: {CustomerName}",
            notification.OrderId, notification.CustomerName);

        // Send confirmation email, update cache, trigger workflow, etc.
        await _emailService.SendOrderConfirmationAsync(notification.OrderId, notification.CustomerName, cancellationToken);
    }
}

// 3. Handler is auto-registered by MediatR assembly scan in AddApplication()
// 4. Fires automatically after successful SaveChanges()
```

### Pagination with Result<T>
```csharp
// Query
public sealed record GetProductListQuery(int PageNumber = 1, int PageSize = 10) : IRequest<Result<PagedList<ProductDto>>>;

// Handler
public sealed class GetProductListQueryHandler(
    IRepository<Product, ProductId> repository) : IRequestHandler<GetProductListQuery, Result<PagedList<ProductDto>>>
{
    public async Task<Result<PagedList<ProductDto>>> Handle(GetProductListQuery request, CancellationToken ct)
    {
        var spec = new ActiveProductsSpecification();
        var products = await repository.ListAsync(spec, ct);
        var count = await repository.CountAsync(spec, ct);

        var dtos = products
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto(p.Id.Value, p.Name, p.Price))
            .ToList();

        return Result<PagedList<ProductDto>>.Success(
            new PagedList<ProductDto>(dtos, count, request.PageNumber, request.PageSize));
    }
}

// Response in API
var result = await mediator.Send(new GetProductListQuery(pageNumber, pageSize), ct);
if (!result.IsSuccess)
    return BadRequest(result.Error);

return Ok(new { data = result.Value });
```
