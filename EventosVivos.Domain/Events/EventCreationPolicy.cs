using EventosVivos.Domain.Primitives;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Domain.Events;

public static class EventCreationPolicy
{
    private static readonly TimeSpan WeekendStartLimit = TimeSpan.FromHours(22);

    public static void EnsureCanCreate(EventSchedule schedule, DateTimeOffset now)
    {
        now = Guard.AgainstDefault(now, nameof(now));

        if (schedule.StartsAt <= now)
        {
            throw new DomainException("El evento debe iniciar en el futuro.");
        }

        if (IsWeekendAfterLimit(schedule.StartsAt))
        {
            throw new DomainException("Los eventos de fin de semana no pueden iniciar desde las 22:00.");
        }
    }

    private static bool IsWeekendAfterLimit(DateTimeOffset startsAt)
    {
        return (startsAt.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) &&
            startsAt.TimeOfDay >= WeekendStartLimit;
    }
}
