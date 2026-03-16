namespace DotnetApiDddTemplate.Domain.Common.Interfaces;

/// <summary>
/// Marker interface for strongly typed IDs.
/// Implemented as readonly record struct { Guid Value }.
/// Provides type safety for identifiers.
/// </summary>
public interface IStronglyTypedId
{
    Guid Value { get; }
}
