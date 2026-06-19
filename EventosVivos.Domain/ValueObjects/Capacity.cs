using EventosVivos.Domain.Primitives;

namespace EventosVivos.Domain.ValueObjects;

public readonly record struct Capacity
{
    public Capacity(int value)
    {
        Value = Guard.AgainstNonPositive(value, nameof(value));
    }

    public int Value { get; }

    public bool CanContain(TicketQuantity quantity)
    {
        return Value >= quantity.Value;
    }
}
