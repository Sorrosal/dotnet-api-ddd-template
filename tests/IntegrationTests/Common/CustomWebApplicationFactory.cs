namespace DotnetApiDddTemplate.IntegrationTests.Common;

/// <summary>
/// Custom web application factory for integration tests.
/// Configures test database with Testcontainers PostgreSQL.
/// </summary>
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
            // Remove default DbContext configuration
            var dbContextDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            // Add test DbContext pointing to testcontainer
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));

            // Configure test settings
            services.AddScoped<ICurrentUser, TestCurrentUser>();
        });

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override configuration for tests
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Secret", "test-secret-key-very-long-for-testing-only" },
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

/// <summary>
/// Mock implementation of ICurrentUser for tests.
/// </summary>
public sealed class TestCurrentUser : ICurrentUser
{
    public string? Id => "test-user";
    public string? Email => "test@example.com";
    public string? Name => "Test User";
    public bool IsAuthenticated => true;
}
