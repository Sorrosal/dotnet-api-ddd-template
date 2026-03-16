namespace DotnetApiDddTemplate.Domain.Common;

/// <summary>
/// Marker interface for entities that have domain events.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}

/// <summary>
/// Base class for all domain entities.
/// Provides ID and domain event collection.
/// </summary>
public abstract class BaseEntity<TId> : IEquatable<BaseEntity<TId>>, IHasDomainEvents
    where TId : struct
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public TId Id { get; protected set; }

    /// <summary>
    /// Collection of domain events raised by this entity.
    /// Events are dispatched before SaveChanges by interceptor.
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Get copy of domain events.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Clear all domain events (called after dispatch).
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Raise a domain event.
    /// </summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public override bool Equals(object? obj) => obj is BaseEntity<TId> entity && Id.Equals(entity.Id);

    public bool Equals(BaseEntity<TId>? other) => other is not null && Id.Equals(other.Id);

    public override int GetHashCode() => Id.GetHashCode();
}
