namespace DotnetApiDddTemplate.Domain.Common;

/// <summary>
/// Base class for value objects.
/// Provides structural equality based on components.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Get equality components for structural comparison.
    /// </summary>
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj) =>
        obj is ValueObject valueObject && ValuesAreEqual(valueObject);

    public override int GetHashCode() =>
        GetEqualityComponents()
            .Aggregate(default(int), (hashcode, value) =>
                HashCode.Combine(hashcode, value.GetHashCode()));

    public bool Equals(ValueObject? other) =>
        other is not null && ValuesAreEqual(other);

    public static bool operator ==(ValueObject left, ValueObject right) =>
        left.Equals(right);

    public static bool operator !=(ValueObject left, ValueObject right) =>
        !left.Equals(right);

    private bool ValuesAreEqual(ValueObject other) =>
        GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
}
