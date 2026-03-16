namespace DotnetApiDddTemplate.Domain.Customers.Events;

/// <summary>
/// Domain event raised when a customer is updated.
/// </summary>
public sealed record CustomerUpdatedDomainEvent(CustomerId CustomerId, string Name, string Email) : IDomainEvent;
