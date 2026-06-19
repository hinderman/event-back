using EventosVivos.Domain.Primitives;

namespace EventosVivos.Domain.ValueObjects;

public readonly record struct PasswordHash
{
    public PasswordHash(string value)
    {
        value = Guard.AgainstBlank(value, nameof(value));
        Value = Guard.AgainstLength(value, 255, nameof(value));
    }

    public string Value { get; }

    public override string ToString()
    {
        return Value;
    }
}
