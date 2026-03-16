namespace DotnetApiDddTemplate.Domain.Customers.Events;

/// <summary>
/// Domain event raised when a customer is deleted.
/// </summary>
public sealed record CustomerDeletedDomainEvent(CustomerId CustomerId) : IDomainEvent;
