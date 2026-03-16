namespace DotnetApiDddTemplate.Application.Features.Customers.Events;

/// <summary>
/// Domain event handler for CustomerCreatedDomainEvent.
/// Performs side effects when a customer is created (logging, notifications, etc).
/// </summary>
public sealed class CustomerCreatedDomainEventHandler(
    ILogger<CustomerCreatedDomainEventHandler> logger) : INotificationHandler<CustomerCreatedDomainEvent>
{
    public Task Handle(
        CustomerCreatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Customer {CustomerId} created: {CustomerName} ({Email})",
            notification.CustomerId.Value,
            notification.Name,
            notification.Email);

        // TODO: Send welcome email
        // TODO: Update read models
        // TODO: Send integration event to other bounded contexts

        return Task.CompletedTask;
    }
}
