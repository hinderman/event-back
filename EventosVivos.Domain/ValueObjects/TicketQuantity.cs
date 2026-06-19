using EventosVivos.Domain.Primitives;

namespace EventosVivos.Domain.ValueObjects;

public readonly record struct TicketQuantity
{
    public TicketQuantity(int value)
    {
        Value = Guard.AgainstNonPositive(value, nameof(value));
    }

    public int Value { get; }
}
