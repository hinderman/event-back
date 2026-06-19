using EventosVivos.Domain.Primitives;

namespace EventosVivos.Domain.ValueObjects;

public readonly record struct EventSchedule
{
    public EventSchedule(DateTimeOffset startsAt, DateTimeOffset endsAt)
    {
        StartsAt = Guard.AgainstDefault(startsAt, nameof(startsAt));
        EndsAt = Guard.AgainstDefault(endsAt, nameof(endsAt));

        if (EndsAt <= StartsAt)
        {
            throw new DomainException("La fecha final del evento debe ser posterior a la fecha inicial.");
        }
    }

    public DateTimeOffset StartsAt { get; }

    public DateTimeOffset EndsAt { get; }

    public bool Overlaps(EventSchedule other)
    {
        return StartsAt < other.EndsAt && EndsAt > other.StartsAt;
    }
}
