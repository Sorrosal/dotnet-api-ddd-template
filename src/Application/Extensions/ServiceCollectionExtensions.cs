namespace DotnetApiDddTemplate.Application.Extensions;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection configuration extensions for Application layer.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register Application layer services.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
        });

        // Register individual validators
        services.AddScoped<FluentValidation.IValidator<RegisterCommand>, RegisterCommandValidator>();
        services.AddScoped<FluentValidation.IValidator<LoginCommand>, LoginCommandValidator>();
        services.AddScoped<FluentValidation.IValidator<RefreshTokenCommand>, RefreshTokenCommandValidator>();
        services.AddScoped<FluentValidation.IValidator<LogoutCommand>, LogoutCommandValidator>();
        services.AddScoped<FluentValidation.IValidator<CreateCustomerCommand>, CreateCustomerCommandValidator>();
        services.AddScoped<FluentValidation.IValidator<UpdateCustomerCommand>, UpdateCustomerCommandValidator>();
        services.AddScoped<FluentValidation.IValidator<DeleteCustomerCommand>, DeleteCustomerCommandValidator>();

        // Add validation behavior
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return services;
    }
}
