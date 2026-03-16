# Best Practices Implementation Guide

Complete guide to implementing all critical and recommended best practices for the DotnetApiDddTemplate.

**Stack:** .NET 10 LTS | PostgreSQL | EF Core 10 | MediatR | FluentValidation | ILogger

---

## 📋 Table of Contents

1. [Specification Pattern](#1-specification-pattern)
2. [Correlation ID / Distributed Tracing](#2-correlation-id--distributed-tracing)
3. [Soft Delete / Logical Delete](#3-soft-delete--logical-delete)
4. [Entity Auditing](#4-entity-auditing)
5. [OpenAPI / Swagger Documentation](#5-openapi--swagger-documentation)
6. [Authorization / Authentication Policies](#6-authorization--authentication-policies)
7. [Optimistic Concurrency Control](#7-optimistic-concurrency-control)
8. [Exception Handling Strategy](#8-exception-handling-strategy)
9. [Health Checks](#9-health-checks)
10. [Request/Response Logging Middleware](#10-requestresponse-logging-middleware)
11. [Configuration Management](#11-configuration-management)
12. [Transactional Consistency](#12-transactional-consistency)
13. [Validation Rules Reusability](#13-validation-rules-reusability)
14. [Architectural Testing](#14-architectural-testing)

---

## 1. Specification Pattern

**Purpose:** Encapsulate complex query logic, making it reusable and testable.

### Files to Create

```
src/Domain/Common/Specifications/
├── ISpecification.cs
├── Specification.cs
└── SpecificationEvaluator.cs

src/Application/Features/Orders/Specifications/
├── OrderByStatusSpecification.cs
├── ActiveOrdersSpecification.cs
└── ... (other specifications)
```

### Implementation

**Domain/Common/Specifications/ISpecification.cs**
```csharp
namespace DotnetApiDddTemplate.Domain.Common.Specifications;

/// <summary>
/// Interface for specification pattern implementations.
/// Encapsulates query logic in a reusable, testable manner.
/// </summary>
public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    int? Take { get; }
    int? Skip { get; }
    bool IsPagingEnabled { get; }
}
```

**Domain/Common/Specifications/Specification.cs**
```csharp
namespace DotnetApiDddTemplate.Domain.Common.Specifications;

public abstract class Specification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; protected set; }
    public List<Expression<Func<T, object>>> Includes { get; } = [];
    public Expression<Func<T, object>>? OrderBy { get; protected set; }
    public Expression<Func<T, object>>? OrderByDescending { get; protected set; }
    public int? Take { get; protected set; }
    public int? Skip { get; protected set; }
    public bool IsPagingEnabled { get; protected set; }

    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected virtual void ApplyPaging(int pageNumber, int pageSize)
    {
        Skip = (pageNumber - 1) * pageSize;
        Take = pageSize;
        IsPagingEnabled = true;
    }
}
```

**Infrastructure/Persistence/Specifications/SpecificationEvaluator.cs**
```csharp
namespace DotnetApiDddTemplate.Infrastructure.Persistence.Specifications;

public sealed class SpecificationEvaluator<T> where T : class
{
    public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> specification)
    {
        var query = inputQuery;

        // Apply filtering
        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply includes
        query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Apply paging
        if (specification.IsPagingEnabled)
        {
            if (specification.Skip.HasValue)
                query = query.Skip(specification.Skip.Value);

            if (specification.Take.HasValue)
                query = query.Take(specification.Take.Value);
        }

        return query;
    }
}
```

**Application/Features/Orders/Specifications/ActiveOrdersSpecification.cs**
```csharp
namespace DotnetApiDddTemplate.Application.Features.Orders.Specifications;

public sealed class ActiveOrdersSpecification : Specification<Order>
{
    public ActiveOrdersSpecification(int pageNumber, int pageSize)
    {
        Criteria = o => !o.IsDeleted && o.Status != OrderStatusEnum.Cancelled;
        AddInclude(o => o.Items);
        OrderByDescending = o => o.CreatedAt;
        ApplyPaging(pageNumber, pageSize);
    }
}
```

### Usage in Repository

```csharp
// Infrastructure/Persistence/Repositories/OrderRepository.cs
public sealed class OrderRepository(ApplicationDbContext context) : GenericRepository<Order, OrderId>(context)
{
    public async Task<List<Order>> GetBySpecificationAsync(
        ISpecification<Order> specification,
        CancellationToken ct = default)
    {
        return await SpecificationEvaluator<Order>.GetQuery(Context.Orders, specification)
            .ToListAsync(ct);
    }
}

// Application/Features/Orders/Queries/GetOrderList/GetOrderListQueryHandler.cs
public sealed class GetOrderListQueryHandler(
    IOrderRepository orderRepository,
    ILogger<GetOrderListQueryHandler> logger)
    : IRequestHandler<GetOrderListQuery, Result<PagedList<GetOrderListResponse>>>
{
    public async Task<Result<PagedList<GetOrderListResponse>>> Handle(
        GetOrderListQuery request,
        CancellationToken ct)
    {
        logger.LogInformation("Fetching orders - Page: {PageNumber}, Size: {PageSize}",
            request.PageNumber, request.PageSize);

        var specification = new OrderByStatusSpecification(request.Status, request.PageNumber, request.PageSize);
        var orders = await orderRepository.GetBySpecificationAsync(specification, ct);

        var response = orders.Select(o => new GetOrderListResponse(o.Id.Value, o.Name)).ToList();
        return Result<PagedList<GetOrderListResponse>>.Success(new PagedList<GetOrderListResponse>(response, orders.Count, request.PageNumber, request.PageSize));
    }
}
```

---

## 2. Correlation ID / Distributed Tracing

**Purpose:** Track requests across logs and microservices for debugging and monitoring.

### Files to Create

```
src/Api/Middleware/
├── CorrelationIdMiddleware.cs
└── RequestContextMiddleware.cs

src/Application/Common/Interfaces/
└── IRequestContext.cs

src/Infrastructure/Services/
└── RequestContextService.cs
```

### Implementation

**Api/Middleware/CorrelationIdMiddleware.cs**
```csharp
namespace DotnetApiDddTemplate.Api.Middleware;

/// <summary>
/// Middleware that adds/extracts correlation ID for request tracing.
/// Correlation ID enables tracking requests across logs and microservices.
/// </summary>
public sealed class CorrelationIdMiddleware(
    RequestDelegate next,
    ILogger<CorrelationIdMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        const string correlationIdHeader = "X-Correlation-ID";

        var correlationId = context.Request.Headers.TryGetValue(
            correlationIdHeader,
            out var id)
            ? id.ToString()
            : Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.Add(correlationIdHeader, correlationId);

        using (logger.BeginScope(new Dictionary<string, object>
        {
            { "CorrelationId", correlationId },
            { "RequestPath", context.Request.Path },
            { "RequestMethod", context.Request.Method }
        }))
        {
            logger.LogInformation(
                "Request started - Method: {Method}, Path: {Path}",
                context.Request.Method,
                context.Request.Path);

            try
            {
                await next(context);
            }
            finally
            {
                logger.LogInformation(
                    "Request completed - StatusCode: {StatusCode}",
                    context.Response.StatusCode);
            }
        }
    }
}
```

**Application/Common/Interfaces/IRequestContext.cs**
```csharp
namespace DotnetApiDddTemplate.Application.Common.Interfaces;

public interface IRequestContext
{
    string CorrelationId { get; }
    Guid UserId { get; }
    string UserEmail { get; }
}
```

**Infrastructure/Services/RequestContextService.cs**
```csharp
namespace DotnetApiDddTemplate.Infrastructure.Services;

public sealed class RequestContextService(IHttpContextAccessor httpContextAccessor)
    : IRequestContext
{
    public string CorrelationId =>
        httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString()
        ?? Guid.NewGuid().ToString();

    public Guid UserId =>
        Guid.TryParse(
            httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            out var id)
        ? id
        : Guid.Empty;

    public string UserEmail =>
        httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value
        ?? string.Empty;
}
```

### Usage

```csharp
// Program.cs
app.UseMiddleware<CorrelationIdMiddleware>();

services.AddScoped<IRequestContext, RequestContextService>();

// In handlers/services
public sealed class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IRequestContext requestContext,
    ILogger<CreateOrderCommandHandler> logger)
    : IRequestHandler<CreateOrderCommand, Result<OrderId>>
{
    public async Task<Result<OrderId>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        logger.LogInformation(
            "Creating order for user {UserId} with correlation ID {CorrelationId}",
            requestContext.UserId,
            requestContext.CorrelationId);

        // ...
    }
}
```

---

## 3. Soft Delete / Logical Delete

**Purpose:** Keep historical data without physical deletion, maintain referential integrity.

### Implementation

**Domain/Common/BaseEntity.cs (Update)**
```csharp
namespace DotnetApiDddTemplate.Domain.Common;

public abstract class BaseEntity<TId> where TId : struct, IStronglyTypedId
{
    public TId Id { get; protected set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Indicates if the entity is logically deleted.
    /// </summary>
    public bool IsDeleted => DeletedAt.HasValue;

    protected List<IDomainEvent> DomainEvents { get; } = [];
    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => DomainEvents.AsReadOnly();
    public void ClearDomainEvents() => DomainEvents.Clear();
}
```

**Infrastructure/Persistence/Interceptors/SoftDeleteInterceptor.cs**
```csharp
namespace DotnetApiDddTemplate.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that handles soft deletes by setting DeletedAt instead of removing records.
/// </summary>
public sealed class SoftDeleteInterceptor(ILogger<SoftDeleteInterceptor> logger)
    : SaveChangesInterceptor
{
    public override ValueTask<int> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is not BaseEntity<dynamic> entity)
                continue;

            if (entry.State != EntityState.Deleted)
                continue;

            entry.State = EntityState.Modified;
            entity.DeletedAt = DateTime.UtcNow;

            var entityType = entry.Entity.GetType().Name;
            logger.LogInformation("Soft delete executed for {EntityType} with ID {EntityId}",
                entityType, entity.Id);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
```

**Infrastructure/Persistence/ApplicationDbContext.cs (Update)**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Global query filter: exclude soft-deleted entities
    var entityTypes = modelBuilder.Model.GetEntityTypes()
        .Where(t => typeof(BaseEntity<dynamic>).IsAssignableFrom(t.ClrType))
        .ToList();

    foreach (var entityType in entityTypes)
    {
        var parameter = Expression.Parameter(entityType.ClrType, "e");
        var property = Expression.Property(parameter, "DeletedAt");
        var nullCheck = Expression.Equal(property, Expression.Constant(null));
        var lambda = Expression.Lambda(nullCheck, parameter);

        modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
    }

    base.OnModelCreating(modelBuilder);
}
```

---

## 4. Entity Auditing

**Purpose:** Track who changed what and when for compliance and debugging.

### Files to Create

```
src/Domain/Common/
└── AuditableEntity.cs

src/Infrastructure/Persistence/Interceptors/
└── AuditableInterceptor.cs
```

### Implementation

**Domain/Common/AuditableEntity.cs**
```csharp
namespace DotnetApiDddTemplate.Domain.Common;

/// <summary>
/// Base class for entities that track creation and modification audit information.
/// </summary>
public abstract class AuditableEntity<TId> : BaseEntity<TId> where TId : struct, IStronglyTypedId
{
    /// <summary>
    /// ID of the user who created this entity.
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Timestamp when this entity was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ID of the user who last modified this entity.
    /// </summary>
    public Guid? ModifiedBy { get; set; }

    /// <summary>
    /// Timestamp of the last modification.
    /// </summary>
    public DateTime? ModifiedAtUtc { get; set; }
}
```

**Infrastructure/Persistence/Interceptors/AuditableInterceptor.cs**
```csharp
namespace DotnetApiDddTemplate.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that automatically sets audit fields (CreatedBy, ModifiedBy, timestamps).
/// </summary>
public sealed class AuditableInterceptor(
    ICurrentUser currentUser,
    ILogger<AuditableInterceptor> logger)
    : SaveChangesInterceptor
{
    public override ValueTask<int> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        var utcNow = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity<dynamic>>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = currentUser.Id;
                entry.Entity.CreatedAtUtc = utcNow;

                logger.LogInformation(
                    "Entity created - Type: {EntityType}, CreatedBy: {UserId}",
                    entry.Entity.GetType().Name,
                    currentUser.Id);
            }
            else if (entry.State == EntityState.Modified && entry.HasChangedOwnedProperties())
            {
                entry.Entity.ModifiedBy = currentUser.Id;
                entry.Entity.ModifiedAtUtc = utcNow;

                logger.LogInformation(
                    "Entity modified - Type: {EntityType}, ModifiedBy: {UserId}",
                    entry.Entity.GetType().Name,
                    currentUser.Id);
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
```

**Usage Example:**
```csharp
// Domain/Orders/Entities/Order.cs
public sealed class Order : AuditableEntity<OrderId>
{
    public string Name { get; private set; } = string.Empty;
    // CreatedBy, CreatedAtUtc, ModifiedBy, ModifiedAtUtc are inherited
    // DeletedAt is inherited (soft delete)
}
```

---

## 5. OpenAPI / Swagger Documentation

**Purpose:** Interactive, auto-generated API documentation.

### Files to Create

```
src/Api/
├── Extensions/
│   └── OpenApiExtensions.cs
└── Filters/
    ├── SwaggerOperationFilter.cs
    └── SwaggerSchemaFilter.cs
```

### Implementation

**Api/Extensions/OpenApiExtensions.cs**
```csharp
namespace DotnetApiDddTemplate.Api.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiDocumentation(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            // Document each API version
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "DotnetApiDddTemplate API",
                Version = "1.0",
                Description = "REST API built with DDD, CQRS, and Clean Architecture",
                Contact = new OpenApiContact
                {
                    Name = "Development Team",
                    Email = "dev@example.com"
                },
                License = new OpenApiLicense
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Include XML comments from controllers and models
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Add bearer token authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter your JWT token"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Custom operation and schema filters
            options.OperationFilter<SwaggerOperationFilter>();
            options.SchemaFilter<SwaggerSchemaFilter>();
        });

        return services;
    }

    public static WebApplication UseOpenApiDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
            options.RoutePrefix = "swagger";
            options.DefaultModelsExpandDepth(2);
            options.DefaultModelExpandDepth(2);
        });

        return app;
    }
}
```

**Api/Filters/SwaggerOperationFilter.cs**
```csharp
namespace DotnetApiDddTemplate.Api.Filters;

public sealed class SwaggerOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Add correlation ID parameter to all operations
        operation.Parameters ??= [];

        operation.Parameters.Insert(0, new OpenApiParameter
        {
            Name = "X-Correlation-ID",
            In = ParameterLocation.Header,
            Description = "Unique identifier for request tracing",
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Format = "uuid"
            }
        });

        // Add response for 500 error to all operations
        var statusCode = "500";
        operation.Responses ??= [];

        if (!operation.Responses.ContainsKey(statusCode))
        {
            operation.Responses.Add(statusCode, new OpenApiResponse
            {
                Description = "Internal Server Error",
                Content = new Dictionary<string, MediaTypeObject>
                {
                    {
                        "application/json",
                        new MediaTypeObject
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    { "code", new OpenApiSchema { Type = "string" } },
                                    { "message", new OpenApiSchema { Type = "string" } },
                                    { "traceId", new OpenApiSchema { Type = "string" } }
                                }
                            }
                        }
                    }
                }
            });
        }
    }
}
```

**Api/Filters/SwaggerSchemaFilter.cs**
```csharp
namespace DotnetApiDddTemplate.Api.Filters;

public sealed class SwaggerSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // Remove nullable annotations for better Swagger UI
        if (schema.Properties is null)
            return;

        foreach (var prop in schema.Properties.Values)
        {
            if (prop.Nullable && prop.Default is null)
            {
                prop.Nullable = false;
            }
        }
    }
}
```

**Controller with XML Documentation:**
```csharp
namespace DotnetApiDddTemplate.Api.Controllers.V1;

/// <summary>
/// Order management endpoints.
/// </summary>
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public sealed class OrdersController(
    ISender mediator,
    ILogger<OrdersController> logger) : ControllerBase
{
    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="request">The order creation request details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The ID of the created order.</returns>
    /// <response code="201">Order created successfully.</response>
    /// <response code="400">Invalid order data provided.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpPost]
    [Authorize]
    [ProduceResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProduceResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken ct)
    {
        var command = new CreateOrderCommand(request.Name, request.Description);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { orderId = result.Value }, result.Value)
            : BadRequest(new ErrorResponse("Invalid.Order", result.Error.Message));
    }
}
```

**Program.cs Integration:**
```csharp
// Add before build
builder.Services.AddOpenApiDocumentation();

// Add after build
app.UseOpenApiDocumentation();
```

---

## 6. Authorization / Authentication Policies

**Purpose:** Role-based and claim-based access control.

### Files to Create

```
src/Application/Common/Interfaces/
└── ICurrentUser.cs

src/Infrastructure/Services/
└── CurrentUserService.cs

src/Api/
├── Extensions/
│   └── AuthorizationExtensions.cs
└── Constants/
    └── AuthorizationPolicies.cs
```

### Implementation

**Application/Common/Interfaces/ICurrentUser.cs**
```csharp
namespace DotnetApiDddTemplate.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid Id { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
    IReadOnlyList<string> Roles { get; }
    IReadOnlyList<string> Permissions { get; }
    bool HasRole(string role);
    bool HasPermission(string permission);
}
```

**Infrastructure/Services/CurrentUserService.cs**
```csharp
namespace DotnetApiDddTemplate.Infrastructure.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUser
{
    private readonly ClaimsPrincipal? _user = httpContextAccessor.HttpContext?.User;

    public Guid Id =>
        Guid.TryParse(_user?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id)
            ? id
            : Guid.Empty;

    public string Email =>
        _user?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

    public bool IsAuthenticated =>
        _user?.Identity?.IsAuthenticated ?? false;

    public IReadOnlyList<string> Roles =>
        _user?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? [];

    public IReadOnlyList<string> Permissions =>
        _user?.FindAll("permission").Select(c => c.Value).ToList() ?? [];

    public bool HasRole(string role) =>
        Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    public bool HasPermission(string permission) =>
        Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
}
```

**Api/Constants/AuthorizationPolicies.cs**
```csharp
namespace DotnetApiDddTemplate.Api.Constants;

public static class AuthorizationPolicies
{
    public const string AdminOnly = nameof(AdminOnly);
    public const string OrderManagement = nameof(OrderManagement);
    public const string ProductManagement = nameof(ProductManagement);
    public const string ReadOnly = nameof(ReadOnly);
}
```

**Api/Extensions/AuthorizationExtensions.cs**
```csharp
namespace DotnetApiDddTemplate.Api.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddCustomAuthorization(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtSecret = configuration["Jwt:Secret"]
                    ?? throw new InvalidOperationException("JWT:Secret not configured");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSecret)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.AdminOnly,
                policy => policy.RequireRole("Admin"))
            .AddPolicy(AuthorizationPolicies.OrderManagement,
                policy => policy.RequireClaim("permission", "manage_orders"))
            .AddPolicy(AuthorizationPolicies.ProductManagement,
                policy => policy.RequireClaim("permission", "manage_products"))
            .AddPolicy(AuthorizationPolicies.ReadOnly,
                policy => policy.RequireRole("User", "Admin"));

        return services;
    }
}
```

**Usage in Controllers:**
```csharp
/// <summary>
/// Deletes an order (Admin only).
/// </summary>
[HttpDelete("{orderId}")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[ProduceResponseType(StatusCodes.Status204NoContent)]
[ProduceResponseType(StatusCodes.Status403Forbidden)]
public async Task<IActionResult> Delete(Guid orderId, CancellationToken ct)
{
    var command = new DeleteOrderCommand(orderId);
    var result = await mediator.Send(command, ct);
    return result.IsSuccess ? NoContent() : Forbid();
}

/// <summary>
/// Gets all orders (Read-only access).
/// </summary>
[HttpGet]
[Authorize(Policy = AuthorizationPolicies.ReadOnly)]
public async Task<IActionResult> GetList(CancellationToken ct)
{
    var query = new GetOrderListQuery();
    var result = await mediator.Send(query, ct);
    return Ok(result.Value);
}
```

---

## 7. Optimistic Concurrency Control

**Purpose:** Prevent race conditions by detecting concurrent modifications.

### Implementation

**Domain/Common/BaseEntity.cs (Update)**
```csharp
public abstract class BaseEntity<TId> where TId : struct, IStronglyTypedId
{
    public TId Id { get; protected set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Version number for optimistic concurrency control.
    /// Incremented on each modification by EF Core.
    /// </summary>
    [Timestamp]
    public byte[] RowVersion { get; set; } = [];

    public bool IsDeleted => DeletedAt.HasValue;

    protected List<IDomainEvent> DomainEvents { get; } = [];

    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => DomainEvents.AsReadOnly();
    public void ClearDomainEvents() => DomainEvents.Clear();
}
```

**Infrastructure/Persistence/Configurations/OrderConfiguration.cs (Example)**
```csharp
namespace DotnetApiDddTemplate.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(id => id.Value, value => new OrderId(value));

        builder.Property(o => o.Name)
            .HasMaxLength(200)
            .IsRequired();

        // Configure row version for optimistic concurrency
        builder.Property(o => o.RowVersion)
            .IsRowVersion();

        // Configure soft delete
        builder.Property(o => o.DeletedAt);

        // Configure auditing
        builder.Property(o => o.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");

        builder.HasQueryFilter(o => !o.IsDeleted);

        // Relationships
        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**Handling Concurrency Exceptions:**
```csharp
// Application/Common/Behaviors/UnitOfWorkBehavior.cs
public sealed class UnitOfWorkBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    ILogger<UnitOfWorkBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var response = await next();

        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entityName = ex.Entries.FirstOrDefault()?.Entity.GetType().Name ?? "Entity";

            logger.LogError(
                "Concurrency conflict on {EntityName}: {Message}",
                entityName,
                ex.Message);

            // If TResponse is Result-based, return failure
            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition().Name.Contains("Result"))
            {
                var resultType = typeof(TResponse);
                var method = resultType.GetMethod("Failure",
                    [typeof(Error)]);

                if (method is not null)
                {
                    var concurrencyError = new Error(
                        "Concurrency.Conflict",
                        $"The {entityName} was modified by another user. Please refresh and try again.");

                    return (TResponse)method.Invoke(null, [concurrencyError])!;
                }
            }

            throw;
        }

        return response;
    }
}
```

---

## 8. Exception Handling Strategy

**Purpose:** Handle unexpected exceptions gracefully and securely.

### Files to Create

```
src/Api/Middleware/
├── GlobalExceptionHandlerMiddleware.cs
└── ExceptionHandlerExtensions.cs

src/Api/Models/
└── ErrorResponse.cs

src/Application/Common/Exceptions/
├── ApplicationException.cs
├── ValidationException.cs
└── NotFoundException.cs
```

### Implementation

**Application/Common/Exceptions/ApplicationException.cs**
```csharp
namespace DotnetApiDddTemplate.Application.Common.Exceptions;

/// <summary>
/// Base exception for application-layer exceptions.
/// </summary>
public class ApplicationException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }

    public ApplicationException(
        string message,
        string code = "Application.Error",
        int statusCode = StatusCodes.Status500InternalServerError,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
        StatusCode = statusCode;
    }
}

public sealed class NotFoundException : ApplicationException
{
    public NotFoundException(string message, string code = "NotFound.Error")
        : base(message, code, StatusCodes.Status404NotFound) { }
}

public sealed class ValidationException : ApplicationException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(
        Dictionary<string, string[]> errors,
        string message = "Validation failed")
        : base(message, "Validation.Error", StatusCodes.Status400BadRequest)
    {
        Errors = errors;
    }
}

public sealed class UnauthorizedException : ApplicationException
{
    public UnauthorizedException(string message = "Unauthorized access")
        : base(message, "Unauthorized", StatusCodes.Status401Unauthorized) { }
}

public sealed class ForbiddenException : ApplicationException
{
    public ForbiddenException(string message = "Access forbidden")
        : base(message, "Forbidden", StatusCodes.Status403Forbidden) { }
}
```

**Api/Models/ErrorResponse.cs**
```csharp
namespace DotnetApiDddTemplate.Api.Models;

public sealed record ErrorResponse(
    string Code,
    string Message,
    string? TraceId = null,
    Dictionary<string, string[]>? Errors = null);
```

**Api/Middleware/GlobalExceptionHandlerMiddleware.cs**
```csharp
namespace DotnetApiDddTemplate.Api.Middleware;

/// <summary>
/// Global exception handler middleware that catches unhandled exceptions
/// and returns appropriate HTTP responses.
/// </summary>
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
            await HandleExceptionAsync(context, ex, logger);
        }
    }

    private static Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? context.TraceIdentifier;

        context.Response.ContentType = "application/json";

        var (statusCode, errorResponse) = exception switch
        {
            ApplicationException appEx => (
                appEx.StatusCode,
                new ErrorResponse(
                    appEx.Code,
                    appEx.Message,
                    correlationId,
                    appEx is ValidationException valEx ? valEx.Errors : null)
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                new ErrorResponse(
                    "Internal.Error",
                    "An unexpected error occurred. Please contact support.",
                    correlationId)
            )
        };

        context.Response.StatusCode = statusCode;

        LogException(logger, exception, statusCode, correlationId);

        return context.Response.WriteAsJsonAsync(errorResponse);
    }

    private static void LogException(
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        Exception exception,
        int statusCode,
        string correlationId)
    {
        if (statusCode >= 500)
        {
            logger.LogError(
                exception,
                "Unhandled exception - CorrelationId: {CorrelationId}, StatusCode: {StatusCode}",
                correlationId,
                statusCode);
        }
        else if (statusCode >= 400)
        {
            logger.LogWarning(
                exception,
                "Handled exception - CorrelationId: {CorrelationId}, StatusCode: {StatusCode}",
                correlationId,
                statusCode);
        }
    }
}
```

**Program.cs Integration:**
```csharp
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
```

---

## 9. Health Checks

**Purpose:** Monitor application health for orchestration and monitoring systems.

### Implementation

**Api/Extensions/HealthCheckExtensions.cs**
```csharp
namespace DotnetApiDddTemplate.Api.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddCustomHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(
                name: "Database",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["db", "sql"])
            .AddCheck(
                "API",
                () => HealthCheckResult.Healthy("API is running"),
                tags: ["api", "live"])
            .AddCheck<ReadinessHealthCheck>(
                "Readiness",
                tags: ["readiness"]);

        return services;
    }

    public static WebApplication UseCustomHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = reg => reg.Tags.Contains("live"),
            ResponseWriter = WriteHealthCheckResponse
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = reg => reg.Tags.Contains("readiness"),
            ResponseWriter = WriteHealthCheckResponse
        });

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteDetailedHealthCheckResponse
        });

        return app;
    }

    private static Task WriteHealthCheckResponse(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var response = new { status = report.Status.ToString() };
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task WriteDetailedHealthCheckResponse(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            })
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}

public sealed class ReadinessHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken ct = default)
    {
        // Add readiness checks: all dependencies are initialized, migrations applied, etc.
        return Task.FromResult(HealthCheckResult.Healthy("Service is ready"));
    }
}
```

**Program.cs Integration:**
```csharp
builder.Services.AddCustomHealthChecks(builder.Configuration);

app.UseCustomHealthChecks();
```

---

## 10. Request/Response Logging Middleware

**Purpose:** Complete audit trail of HTTP requests and responses.

### Implementation

**Api/Middleware/RequestResponseLoggingMiddleware.cs**
```csharp
namespace DotnetApiDddTemplate.Api.Middleware;

/// <summary>
/// Middleware that logs detailed request and response information for auditing.
/// </summary>
public sealed class RequestResponseLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestResponseLoggingMiddleware> logger)
{
    private const int MaxBodyLength = 4096;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? context.TraceIdentifier;

        // Log request
        await LogRequestAsync(context, correlationId, logger);

        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        var startTime = DateTime.UtcNow;

        try
        {
            await next(context);
        }
        finally
        {
            // Log response
            await LogResponseAsync(context, responseBody, startTime, correlationId, logger);

            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private static async Task LogRequestAsync(
        HttpContext context,
        string correlationId,
        ILogger logger)
    {
        var request = context.Request;
        var body = string.Empty;

        if (request.ContentLength.HasValue && request.ContentLength > 0)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, leaveOpen: true);
            body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            if (body.Length > MaxBodyLength)
                body = $"{body[..MaxBodyLength]}... (truncated)";
        }

        logger.LogInformation(
            "HTTP Request - CorrelationId: {CorrelationId} | Method: {Method} | Path: {Path} | Headers: {Headers} | Body: {Body}",
            correlationId,
            request.Method,
            request.Path,
            string.Join(", ", request.Headers.Select(h => $"{h.Key}={h.Value}")),
            body);
    }

    private static async Task LogResponseAsync(
        HttpContext context,
        MemoryStream responseBody,
        DateTime startTime,
        string correlationId,
        ILogger logger)
    {
        var response = context.Response;
        var duration = DateTime.UtcNow - startTime;

        var body = string.Empty;
        if (responseBody.Length > 0)
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(responseBody);
            body = await reader.ReadToEndAsync();
            responseBody.Seek(0, SeekOrigin.Begin);

            if (body.Length > MaxBodyLength)
                body = $"{body[..MaxBodyLength]}... (truncated)";
        }

        logger.LogInformation(
            "HTTP Response - CorrelationId: {CorrelationId} | StatusCode: {StatusCode} | Duration: {DurationMs}ms | Body: {Body}",
            correlationId,
            response.StatusCode,
            duration.TotalMilliseconds,
            body);
    }
}
```

**Program.cs Integration:**
```csharp
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
```

---

## 11. Configuration Management

**Purpose:** Secure, environment-specific configuration with secrets management.

### Files to Create

```
src/Infrastructure/Configuration/
├── DatabaseSettings.cs
├── JwtSettings.cs
├── LoggingSettings.cs
└── AppSettings.cs

src/Api/
└── appsettings.*.json
```

### Implementation

**Infrastructure/Configuration/AppSettings.cs**
```csharp
namespace DotnetApiDddTemplate.Infrastructure.Configuration;

public sealed class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
    public int MaxRetryCount { get; set; } = 3;
    public int MaxRetryDelaySeconds { get; set; } = 10;
}

public sealed class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}

public sealed class LoggingSettings
{
    public string LogLevel { get; set; } = "Information";
    public bool EnableConsoleLogging { get; set; } = true;
    public bool EnableFileLogging { get; set; } = false;
    public string? LogFilePath { get; set; }
}

public sealed class AppSettings
{
    public DatabaseSettings Database { get; set; } = new();
    public JwtSettings Jwt { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
}
```

**appsettings.json**
```json
{
  "AppSettings": {
    "Database": {
      "ConnectionString": "Server=localhost;Database=DotnetApiDddTemplate;Port=5432;User Id=postgres;Password=postgres",
      "CommandTimeout": 30,
      "MaxRetryCount": 3,
      "MaxRetryDelaySeconds": 10
    },
    "Jwt": {
      "Secret": "your-secret-key-change-in-production",
      "Issuer": "DotnetApiDddTemplate",
      "Audience": "DotnetApiDddTemplate",
      "ExpirationMinutes": 60
    },
    "Logging": {
      "LogLevel": "Information",
      "EnableConsoleLogging": true,
      "EnableFileLogging": false,
      "LogFilePath": null
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DotnetApiDddTemplate;Port=5432;User Id=postgres;Password=postgres"
  }
}
```

**appsettings.Development.json**
```json
{
  "AppSettings": {
    "Logging": {
      "LogLevel": "Debug",
      "EnableConsoleLogging": true,
      "EnableFileLogging": false
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information"
    }
  }
}
```

**Infrastructure/Extensions/DependencyInjection.cs (Update)**
```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // Bind configuration
    var appSettings = new AppSettings();
    configuration.GetSection("AppSettings").Bind(appSettings);
    services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

    var connectionString = appSettings.Database.ConnectionString
        ?? throw new InvalidOperationException("Database connection string not configured");

    // Database
    services.AddDbContext<ApplicationDbContext>((sp, options) =>
    {
        options.UseNpgsql(
            connectionString,
            x => x.CommandTimeout(appSettings.Database.CommandTimeout)
                   .EnableRetryOnFailure(
                       appSettings.Database.MaxRetryCount,
                       TimeSpan.FromSeconds(appSettings.Database.MaxRetryDelaySeconds),
                       null));

        options.AddInterceptors(
            sp.GetRequiredService<DomainEventDispatcherInterceptor>(),
            sp.GetRequiredService<AuditableInterceptor>(),
            sp.GetRequiredService<SoftDeleteInterceptor>());
    });

    // Services
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    services.AddScoped<ICurrentUser, CurrentUserService>();
    services.AddScoped<IRequestContext, RequestContextService>();

    return services;
}
```

---

## 12. Transactional Consistency

**Purpose:** Explicit control over transaction boundaries and isolation levels.

### Implementation

**Application/Common/Interfaces/IUnitOfWork.cs (Enhanced)**
```csharp
namespace DotnetApiDddTemplate.Application.Common.Interfaces;

public interface IUnitOfWork
{
    Task<T> ExecuteTransactionAsync<T>(
        Func<Task<T>> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
```

**Infrastructure/Persistence/UnitOfWork.cs (Enhanced)**
```csharp
namespace DotnetApiDddTemplate.Infrastructure.Persistence;

public sealed class UnitOfWork(
    ApplicationDbContext context,
    IDomainEventDispatcher eventDispatcher,
    ILogger<UnitOfWork> logger)
    : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public async Task<T> ExecuteTransactionAsync<T>(
        Func<Task<T>> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken ct = default)
    {
        _transaction = await context.Database.BeginTransactionAsync(isolationLevel, ct);

        try
        {
            logger.LogInformation(
                "Transaction started - IsolationLevel: {IsolationLevel}",
                isolationLevel);

            var result = await action();

            await context.SaveChangesAsync(ct);
            await _transaction.CommitAsync(ct);

            logger.LogInformation("Transaction committed successfully");

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Transaction failed, rolling back");
            await _transaction.RollbackAsync(ct);
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            await context.SaveChangesAsync(ct);
            logger.LogInformation("Changes saved to database");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency exception during save");
            throw;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update exception");
            throw;
        }
    }
}
```

**Usage in Handlers:**
```csharp
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
        // Explicit transaction with specific isolation level
        var result = await unitOfWork.ExecuteTransactionAsync(
            async () =>
            {
                var order = Order.Create(request.Name, request.Description);
                if (order.IsFailure)
                    return order;

                await orderRepository.AddAsync(order.Value, ct);
                await unitOfWork.SaveChangesAsync(ct);

                return Result<OrderId>.Success(order.Value.Id);
            },
            IsolationLevel.ReadCommitted,
            ct);

        return result;
    }
}
```

---

## 13. Validation Rules Reusability

**Purpose:** Share validation rules across the application.

### Implementation

**Application/Common/Validators/SharedRules.cs**
```csharp
namespace DotnetApiDddTemplate.Application.Common.Validators;

/// <summary>
/// Shared validation rules that can be reused across validators.
/// </summary>
public static class SharedRules
{
    /// <summary>
    /// Validates a name field (non-empty, max length, valid characters).
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidateName<T>(
        this IRuleBuilder<T, string> rule,
        int maxLength = 200) =>
        rule
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(maxLength).WithMessage($"Name cannot exceed {maxLength} characters")
            .Matches(@"^[a-zA-Z0-9\s\-_.]*$").WithMessage("Name contains invalid characters");

    /// <summary>
    /// Validates an email address.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidateEmail<T>(
        this IRuleBuilder<T, string> rule) =>
        rule
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email format is invalid");

    /// <summary>
    /// Validates a description field.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidateDescription<T>(
        this IRuleBuilder<T, string> rule,
        int minLength = 10,
        int maxLength = 500) =>
        rule
            .NotEmpty().WithMessage("Description is required")
            .MinimumLength(minLength).WithMessage($"Description must be at least {minLength} characters")
            .MaximumLength(maxLength).WithMessage($"Description cannot exceed {maxLength} characters");

    /// <summary>
    /// Validates a positive integer quantity.
    /// </summary>
    public static IRuleBuilderOptions<T, int> ValidateQuantity<T>(
        this IRuleBuilder<T, int> rule) =>
        rule
            .GreaterThan(0).WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(10000).WithMessage("Quantity cannot exceed 10000");

    /// <summary>
    /// Validates a positive price.
    /// </summary>
    public static IRuleBuilderOptions<T, decimal> ValidatePrice<T>(
        this IRuleBuilder<T, decimal> rule) =>
        rule
            .GreaterThan(0).WithMessage("Price must be greater than 0")
            .LessThanOrEqualTo(999999.99m).WithMessage("Price is too high");

    /// <summary>
    /// Validates a GUID ID is not empty.
    /// </summary>
    public static IRuleBuilderOptions<T, Guid> ValidateId<T>(
        this IRuleBuilder<T, Guid> rule) =>
        rule
            .NotEmpty().WithMessage("ID cannot be empty");
}
```

**Usage in Validators:**
```csharp
public sealed class CreateOrderCommandValidator
    : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Name)
            .ValidateName(maxLength: 200);

        RuleFor(x => x.Description)
            .ValidateDescription(minLength: 10, maxLength: 500);

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item");

        RuleForEach(x => x.Items)
            .ChildRules(items =>
            {
                items.RuleFor(i => i.Quantity).ValidateQuantity();
                items.RuleFor(i => i.Price).ValidatePrice();
            });
    }
}

public sealed class CreateProductCommandValidator
    : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).ValidateName();
        RuleFor(x => x.Description).ValidateDescription();
        RuleFor(x => x.Price).ValidatePrice();
    }
}
```

---

## 14. Architectural Testing

**Purpose:** Enforce architectural boundaries and prevent violations.

### Files to Create

```
tests/ArchitectureTests/
├── LayerDependencyTests.cs
├── NamingConventionTests.cs
└── ProjectReferenceTests.cs
```

### Implementation

**ArchitectureTests/LayerDependencyTests.cs**
```csharp
namespace DotnetApiDddTemplate.ArchitectureTests;

public sealed class LayerDependencyTests
{
    private static readonly Assembly DomainAssembly = typeof(Order).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(CreateOrderCommandHandler).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(ApplicationDbContext).Assembly;
    private static readonly Assembly ApiAssembly = typeof(OrdersController).Assembly;

    [Fact]
    public void Domain_Should_NotReferenceOtherProjects()
    {
        var allowedNamespaces = new[]
        {
            "System",
            "System.Collections",
            "System.Linq",
            "System.Threading",
            "DotnetApiDddTemplate.Domain"
        };

        var violations = DomainAssembly.GetReferencedAssemblies()
            .Where(a => !allowedNamespaces.Any(ns => a.Name.StartsWith(ns)))
            .Where(a => !a.Name.StartsWith("System.") && a.Name != "netstandard")
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void Application_Should_NotReferenceInfrastructure()
    {
        var infrastructureName = InfrastructureAssembly.GetName().Name;

        var hasReference = ApplicationAssembly.GetReferencedAssemblies()
            .Any(a => a.Name == infrastructureName);

        Assert.False(hasReference, "Application should not reference Infrastructure");
    }

    [Fact]
    public void Infrastructure_ShouldReference_ApplicationAndDomain()
    {
        var referencedNames = InfrastructureAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        Assert.Contains(ApplicationAssembly.GetName().Name, referencedNames);
        Assert.Contains(DomainAssembly.GetName().Name, referencedNames);
    }

    [Fact]
    public void Api_ShouldReference_ApplicationAndInfrastructure()
    {
        var referencedNames = ApiAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        Assert.Contains(ApplicationAssembly.GetName().Name, referencedNames);
        Assert.Contains(InfrastructureAssembly.GetName().Name, referencedNames);
    }

    [Fact]
    public void Domain_ShouldNotContain_DbContext()
    {
        var hasDbContext = DomainAssembly.GetTypes()
            .Any(t => t.IsAssignableTo(typeof(DbContext)));

        Assert.False(hasDbContext, "Domain should not contain EF Core DbContext");
    }

    [Fact]
    public void Application_ShouldNotContain_Controllers()
    {
        var hasControllers = ApplicationAssembly.GetTypes()
            .Any(t => t.IsAssignableTo(typeof(ControllerBase)));

        Assert.False(hasControllers, "Application should not contain Controllers");
    }
}
```

**ArchitectureTests/NamingConventionTests.cs**
```csharp
namespace DotnetApiDddTemplate.ArchitectureTests;

public sealed class NamingConventionTests
{
    [Fact]
    public void Commands_Should_EndWithCommand()
    {
        var commands = typeof(CreateOrderCommand).Assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IRequest)) &&
                        !t.IsInterface &&
                        t.Namespace?.Contains("Commands") == true)
            .ToList();

        var violations = commands
            .Where(c => !c.Name.EndsWith("Command"))
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void CommandHandlers_Should_EndWithCommandHandler()
    {
        var handlers = typeof(CreateOrderCommandHandler).Assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IRequestHandler<,>)) &&
                        !t.IsInterface)
            .Where(t => t.Namespace?.Contains("Commands") == true)
            .ToList();

        var violations = handlers
            .Where(h => !h.Name.EndsWith("CommandHandler"))
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void Queries_Should_EndWithQuery()
    {
        var queries = typeof(GetOrderByIdQuery).Assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IRequest)) &&
                        !t.IsInterface &&
                        t.Namespace?.Contains("Queries") == true)
            .ToList();

        var violations = queries
            .Where(q => !q.Name.EndsWith("Query"))
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void Entities_Should_BeSealed()
    {
        var entities = typeof(Order).Assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(BaseEntity<>)) &&
                        !t.IsAbstract &&
                        !t.IsInterface)
            .ToList();

        var violations = entities
            .Where(e => !e.IsSealed)
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void Interfaces_Should_StartWithI()
    {
        var interfaces = typeof(IOrderRepository).Assembly.GetTypes()
            .Where(t => t.IsInterface &&
                        !t.Name.StartsWith("<") &&
                        t.Namespace?.StartsWith("DotnetApiDddTemplate") == true)
            .ToList();

        var violations = interfaces
            .Where(i => !i.Name.StartsWith("I"))
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void StronglyTypedIds_Should_HaveSuffix_Id()
    {
        var ids = typeof(OrderId).Assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IStronglyTypedId)) &&
                        !t.IsInterface)
            .ToList();

        var violations = ids
            .Where(i => !i.Name.EndsWith("Id"))
            .ToList();

        Assert.Empty(violations);
    }
}
```

---

## 📋 Summary Checklist

- [ ] Specification Pattern implemented
- [ ] Correlation ID middleware added
- [ ] Soft Delete interceptor configured
- [ ] Entity Auditing implemented
- [ ] Swagger/OpenAPI documentation
- [ ] Authorization policies defined
- [ ] Optimistic Concurrency Control enabled
- [ ] Exception handling middleware
- [ ] Health checks endpoints
- [ ] Request/Response logging
- [ ] Configuration management
- [ ] Transactional consistency
- [ ] Shared validation rules
- [ ] Architectural tests

---

**Last Updated:** 2026-03-16
**Version:** 1.0
**Language:** English
