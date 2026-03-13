---
description: Scaffold integration tests for an API endpoint using WebApplicationFactory and Testcontainers
allowed-tools: Read, Write, Edit, Glob, Grep, Bash
argument-hint: <FeatureName> <ControllerName> [HttpMethod=GET]
---

# Scaffold Integration Tests

Parse `$ARGUMENTS` to extract `FeatureName`, `ControllerName`, and optional `HttpMethod`. If missing, ask the user.

Follow ALL patterns and conventions defined in the root `CLAUDE.md`. Read it first.

## Prerequisites — Ensure Infrastructure Exists

Before creating tests, check if these base classes exist. If not, create them first:

### `tests/IntegrationTests/Common/IntegrationTestWebAppFactory.cs`
```csharp
public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            // Register test database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync() => await _dbContainer.DisposeAsync();
}
```

### `tests/IntegrationTests/Common/BaseIntegrationTest.cs`
```csharp
public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
{
    protected readonly HttpClient Client;
    protected readonly ApplicationDbContext DbContext;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        Client = factory.CreateClient();
        var scope = factory.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }
}
```

## Test File to Create

**Path:** `tests/IntegrationTests/Api/{FeatureName}/{ControllerName}Tests.cs`

```csharp
namespace DotnetApiDddTemplate.IntegrationTests.Api.{FeatureName};

public sealed class {ControllerName}Tests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    private const string BaseUrl = "api/v1/{resource}";

    [Fact]
    public async Task {HttpMethod}_{Resource}_Should_Return200_When_ValidRequest()
    {
        // Arrange — seed database if needed
        // Act — use Client.{GetAsync/PostAsJsonAsync/PutAsJsonAsync/DeleteAsync}
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Deserialize and assert response body
    }

    [Fact]
    public async Task {HttpMethod}_{Resource}_Should_Return400_When_InvalidRequest()
    {
        // Arrange — invalid payload
        // Act
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_{Resource}_Should_Return404_When_NotFound()
    {
        // Arrange — non-existent ID
        // Act
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
```

## Coverage Requirements

Create tests for:
- **Happy path** (2xx) — valid request, verify response body shape and status code.
- **Validation failure** (400) — missing/invalid fields, verify `ProblemDetails` response.
- **Not found** (404) — non-existent resource.
- **Any domain-specific error** cases (e.g. conflict, business rule violations).

## Guidelines

- Each test is independent — use `DbContext` to seed and clean up data.
- Use `System.Net.Http.Json` extension methods: `PostAsJsonAsync`, `GetFromJsonAsync`, `PutAsJsonAsync`.
- Use FluentAssertions for all assertions.
- Avoid shared mutable state between tests — seed fresh data per test.
- Use `Bogus` (`Faker<T>`) for generating realistic test data.
- Verify build: `dotnet build` then `dotnet test tests/IntegrationTests`.
