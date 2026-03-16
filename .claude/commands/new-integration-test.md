---
description: Scaffold integration tests for API endpoints using WebApplicationFactory and Testcontainers with real database
allowed-tools: Read, Write, Edit, Glob, Grep, Bash
argument-hint: <Feature> <ControllerName> [HttpMethod=GET]
---

# Scaffold Integration Tests with Testcontainers

Parse `$ARGUMENTS` to extract `Feature`, `ControllerName`, and optional `HttpMethod`. If missing, ask the user.

Follow patterns in `CLAUDE.md`, `PROJECT_STRUCTURE.md`, and `BEST_PRACTICES.md`.

---

## Prerequisites: Base Test Infrastructure

**Path:** `tests/IntegrationTests/Common/CustomWebApplicationFactory.cs`

```csharp
namespace DotnetApiDddTemplate.IntegrationTests.Common;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("dotnet-api-ddd")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove DbContext
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();

            // Add test DbContext pointing to testcontainer
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));

            // Use in-memory time provider for deterministic tests
            services.AddScoped<ISystemClock>(_ => new SystemClock());
        });

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override config for tests
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Secret", "test-secret-key-for-testing-only" },
                { "Logging:LogLevel:Default", "Warning" }
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Apply migrations
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
```

---

## Integration Test Example

**Path:** `tests/IntegrationTests/Api/{Feature}/{ControllerName}Tests.cs`

```csharp
namespace DotnetApiDddTemplate.IntegrationTests.Api.{Feature};

public sealed class {ControllerName}Tests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = new();
    private HttpClient _httpClient = null!;
    private ApplicationDbContext _dbContext = null!;

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
        _httpClient = _factory.CreateClient();
        _httpClient.DefaultRequestHeaders.Add("X-Correlation-ID", Guid.NewGuid().ToString());

        using var scope = _factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        _httpClient.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Post_Should_Create{Entity}_When_ValidRequest()
    {
        // Arrange
        var request = new
        {
            name = "Test {Entity}",
            description = "Test Description"
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/{controller}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var content = await response.Content.ReadAsAsync<Guid>();
        content.Should().NotBeEmpty();

        // Verify database
        var entity = await _dbContext.{Entities}.FindAsync(new {Entity}Id(content));
        entity.Should().NotBeNull();
        entity!.Name.Should().Be(request.name);
    }

    [Fact]
    public async Task Get_Should_Return{Entity}_When_Exists()
    {
        // Arrange - create entity in database
        var {entity} = {Entity}.Create("Test {Entity}", "Description").Value;
        _dbContext.{Entities}.Add({entity});
        await _dbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync($"/api/v1/{controller}/{{{entity}.Id.Value}}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsAsync<{EntityDto}>();
        content.Name.Should().Be({entity}.Name);
    }

    [Fact]
    public async Task Get_Should_ReturnNotFound_When_DoesNotExist()
    {
        // Act
        var response = await _httpClient.GetAsync($"/api/v1/{controller}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsAsync<ErrorResponse>();
        content.Code.Should().Be("{Entity}.NotFound");
    }

    [Fact]
    public async Task Post_Should_ReturnBadRequest_When_NameEmpty()
    {
        // Arrange
        var request = new { name = "", description = "Test" };

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/{controller}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetHealthCheck_Should_ReturnHealthy()
    {
        // Act
        var response = await _httpClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }
}
```

---

## Key Patterns

✅ **Real Database** - Testcontainers provides actual PostgreSQL
✅ **Clean State** - Each test gets fresh database via Migration
✅ **Correlation ID** - Adds X-Correlation-ID to all requests
✅ **JWT Testing** - Test config provides test secret key
✅ **End-to-End** - Tests full HTTP pipeline (middleware, validation, handlers, persistence)
✅ **Assertions** - FluentAssertions for readable test code
✅ **Database Verification** - Verify changes persisted to database
✅ **Health Checks** - Test /health endpoints

---

## Test Methods

- `Post_Should_Create{Entity}_When_ValidRequest` - Happy path creation
- `Get_Should_Return{Entity}_When_Exists` - Retrieve existing entity
- `Get_Should_ReturnNotFound_When_DoesNotExist` - 404 handling
- `Post_Should_ReturnBadRequest_When_NameEmpty` - Validation failure
- `Delete_Should_ReturnNoContent_When_EntityExists` - Successful deletion
- `Put_Should_UpdateEntity_When_ValidRequest` - Update operation

---

**Reference:** `BEST_PRACTICES.md` - Health Checks, Correlation ID, Exception Handling
