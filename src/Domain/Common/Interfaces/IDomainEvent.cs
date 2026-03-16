namespace DotnetApiDddTemplate.Domain.Common.Interfaces;

/// <summary>
/// Marker interface for domain events.
/// Implemented as sealed records and dispatched by MediatR.
/// </summary>
public interface IDomainEvent : INotification
{
}
