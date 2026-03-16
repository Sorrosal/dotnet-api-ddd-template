namespace DotnetApiDddTemplate.Application.Features.Customers.Events;

/// <summary>
/// Domain event handler for CustomerUpdatedDomainEvent.
/// Performs side effects when a customer is updated.
/// </summary>
public sealed class CustomerUpdatedDomainEventHandler(
    ILogger<CustomerUpdatedDomainEventHandler> logger) : INotificationHandler<CustomerUpdatedDomainEvent>
{
    public Task Handle(
        CustomerUpdatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Customer {CustomerId} updated: {CustomerName} ({Email})",
            notification.CustomerId.Value,
            notification.Name,
            notification.Email);

        // TODO: Update read models
        // TODO: Send notification to related services
        // TODO: Log audit trail

        return Task.CompletedTask;
    }
}
