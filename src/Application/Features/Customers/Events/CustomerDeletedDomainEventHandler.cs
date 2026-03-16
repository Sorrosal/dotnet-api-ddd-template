namespace DotnetApiDddTemplate.Application.Features.Customers.Events;

/// <summary>
/// Domain event handler for CustomerDeletedDomainEvent.
/// Performs side effects when a customer is deleted (soft delete).
/// </summary>
public sealed class CustomerDeletedDomainEventHandler(
    ILogger<CustomerDeletedDomainEventHandler> logger) : INotificationHandler<CustomerDeletedDomainEvent>
{
    public Task Handle(
        CustomerDeletedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Customer {CustomerId} deleted",
            notification.CustomerId.Value);

        // TODO: Notify related services
        // TODO: Clean up associated data
        // TODO: Archive customer data

        return Task.CompletedTask;
    }
}
