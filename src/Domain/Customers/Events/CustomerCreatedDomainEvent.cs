namespace DotnetApiDddTemplate.Domain.Customers.Events;

/// <summary>
/// Domain event raised when a customer is created.
/// </summary>
public sealed record CustomerCreatedDomainEvent(CustomerId CustomerId, string Name, string Email) : IDomainEvent;
