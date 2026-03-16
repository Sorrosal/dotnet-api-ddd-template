namespace DotnetApiDddTemplate.Infrastructure.Extensions;

/// <summary>
/// Dependency injection configuration extensions for Infrastructure layer.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register Infrastructure layer services.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register interceptors first
        services.AddScoped<DomainEventDispatcherInterceptor>();
        services.AddScoped<AuditableInterceptor>();

        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>((provider, options) =>
        {
            options.UseNpgsql(connectionString)
                .AddInterceptors(provider.GetRequiredService<DomainEventDispatcherInterceptor>())
                .AddInterceptors(provider.GetRequiredService<AuditableInterceptor>());
        });

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IPagedCustomerRepository, CustomerRepository>();

        return services;
    }
}
