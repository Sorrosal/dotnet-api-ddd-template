namespace DotnetApiDddTemplate.Infrastructure.Persistence.Interceptors;

using Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
/// EF Core interceptor that dispatches domain events before SaveChanges.
/// Ensures domain events are published after all validations pass.
/// </summary>
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

        // Get all entities that have domain events
        var entries = eventData.Context.ChangeTracker
            .Entries()
            .Where(e => e.Entity is IHasDomainEvents)
            .Cast<EntityEntry>()
            .ToList();

        var domainEvents = new List<IDomainEvent>();

        // Collect all domain events
        foreach (var entry in entries)
        {
            if (entry.Entity is IHasDomainEvents entity)
            {
                domainEvents.AddRange(entity.DomainEvents);
                entity.ClearDomainEvents();
            }
        }

        // Dispatch events
        foreach (var domainEvent in domainEvents)
        {
            logger.LogInformation(
                "Dispatching domain event: {EventType}",
                domainEvent.GetType().Name);

            await mediator.Publish(domainEvent, cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
