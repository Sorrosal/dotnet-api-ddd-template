---
description: Scaffold a new DDD value object with validation, Result pattern, and structural equality
allowed-tools: Read, Write, Edit, Glob, Grep, Bash
argument-hint: <BoundedContext> <ValueObjectName> [property:Type ...]
---

# Scaffold New Value Object with Result Pattern

Parse `$ARGUMENTS` to extract `BoundedContext`, `ValueObjectName`, and properties. If missing, ask the user.

Follow patterns in `CLAUDE.md`, `PROJECT_STRUCTURE.md`, and `BEST_PRACTICES.md`.

---

## File to Create

**Path:** `src/Domain/{BoundedContext}/ValueObjects/{ValueObjectName}.cs`

### Option A — Simple Record (for pure data containers)

```csharp
namespace DotnetApiDddTemplate.Domain.{BoundedContext}.ValueObjects;

/// <summary>
/// Value object: {ValueObjectName}.
/// Immutable, equality by value, no identity.
/// </summary>
public sealed record {ValueObjectName}(
    decimal Amount,
    string Currency);

// If validation needed:
public static Result<{ValueObjectName}> Create(decimal amount, string currency)
{
    if (amount < 0)
        return Result<{ValueObjectName}>.Failure(
            new Error("Money.InvalidAmount", "Amount cannot be negative"));

    if (string.IsNullOrWhiteSpace(currency))
        return Result<{ValueObjectName}>.Failure(
            new Error("Money.InvalidCurrency", "Currency is required"));

    return Result<{ValueObjectName}>.Success(new {ValueObjectName}(amount, currency));
}
```

### Option B — Complex Class (for custom equality or complex logic)

```csharp
namespace DotnetApiDddTemplate.Domain.{BoundedContext}.ValueObjects;

/// <summary>
/// Value object: {ValueObjectName}.
/// Complex validation, custom equality logic.
/// </summary>
public sealed class {ValueObjectName} : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private {ValueObjectName}(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Result<{ValueObjectName}> Create(decimal amount, string currency)
    {
        if (amount < 0)
            return Result<{ValueObjectName}>.Failure(
                new Error("Money.InvalidAmount", "Amount cannot be negative"));

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            return Result<{ValueObjectName}>.Failure(
                new Error("Money.InvalidCurrency", "Currency must be 3 characters (ISO 4217)"));

        return Result<{ValueObjectName}>.Success(new {ValueObjectName}(amount, currency));
    }

    /// <summary>
    /// Add two money objects (same currency required).
    /// </summary>
    public Result<{ValueObjectName}> Add({ValueObjectName} other)
    {
        if (Currency != other.Currency)
            return Result<{ValueObjectName}>.Failure(
                new Error("Money.CurrencyMismatch", "Cannot add different currencies"));

        return Create(Amount + other.Amount, Currency);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

---

## EF Core Configuration

Update the owning entity's configuration:

```csharp
// For owned value objects
builder.OwnsOne(x => x.{PropertyName}, vo =>
{
    vo.Property(m => m.Amount)
        .HasPrecision(18, 2)
        .HasColumnName("Amount");

    vo.Property(m => m.Currency)
        .HasMaxLength(3)
        .HasColumnName("Currency");
});

// For strongly typed IDs (already handled)
// No additional configuration needed
```

---

## Key Patterns

✅ **Immutable** - No setters, init only or private constructors
✅ **Equality by value** - Two objects with same values are equal
✅ **Result<T> validation** - Create() factory returns Result
✅ **No identity** - No ID field
✅ **Encapsulated logic** - Methods that operate on value (Add, Subtract, etc.)
✅ **Structural equality** - Base class or record handles equality
✅ **Strongly typed** - Type-safe instead of string/int primitives

---

**Reference:** `BEST_PRACTICES.md` - Result Pattern, Value Objects design
