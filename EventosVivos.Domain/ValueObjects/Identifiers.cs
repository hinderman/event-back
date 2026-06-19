using EventosVivos.Domain.Primitives;

namespace EventosVivos.Domain.ValueObjects;

public readonly record struct UserTypeId
{
    public UserTypeId(short value)
    {
        Value = Guard.AgainstNegative(value, nameof(value));
    }

    public short Value { get; }

    public static UserTypeId Empty => new(0);
    public bool IsEmpty => Value <= 0;
}

public readonly record struct UserId
{
    public UserId(long value)
    {
        Value = Guard.AgainstNegative(value, nameof(value));
    }

    public long Value { get; }

    public static UserId Empty => new(0);
    public bool IsEmpty => Value <= 0;
}

public readonly record struct EventTypeId
{
    public EventTypeId(short value)
    {
        Value = Guard.AgainstNegative(value, nameof(value));
    }

    public short Value { get; }

    public static EventTypeId Empty => new(0);
    public bool IsEmpty => Value <= 0;
}

public readonly record struct EventStatusId
{
    public EventStatusId(short value)
    {
        Value = Guard.AgainstNegative(value, nameof(value));
    }

    public short Value { get; }

    public static EventStatusId Empty => new(0);
    public bool IsEmpty => Value <= 0;
}

public readonly record struct ReservationStatusId
{
    public ReservationStatusId(short value)
    {
        Value = Guard.AgainstNegative(value, nameof(value));
    }

    public short Value { get; }

    public static ReservationStatusId Empty => new(0);
    public bool IsEmpty => Value <= 0;
}

public readonly record struct VenueId
{
    public VenueId(int value)
    {
        Value = Guard.AgainstNegative(value, nameof(value));
    }

    public int Value { get; }

    public static VenueId Empty => new(0);
    public bool IsEmpty => Value <= 0;
}

public readonly record struct EventId
{
    public EventId(long value)
    {
        Value = Guard.AgainstNegative(value, nameof(value));
    }

    public long Value { get; }

    public static EventId Empty => new(0);
    public bool IsEmpty => Value <= 0;
}

public readonly record struct ReservationId
{
    public ReservationId(long value)
    {
        Value = Guard.AgainstNegative(value, nameof(value));
    }

    public long Value { get; }

    public static ReservationId Empty => new(0);
    public bool IsEmpty => Value <= 0;
}
