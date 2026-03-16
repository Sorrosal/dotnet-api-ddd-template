namespace DotnetApiDddTemplate.Domain.Customers.ValueObjects;

public readonly record struct CustomerId(Guid Value) : IStronglyTypedId;
