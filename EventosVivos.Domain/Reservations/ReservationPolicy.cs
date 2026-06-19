using EventosVivos.Domain.Primitives;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Domain.Reservations;

public static class ReservationPolicy
{
    public const int MinimumHoursBeforeStart = 1;
    public const int NearEventWindowHours = 24;
    public const int MaximumTicketsNearEvent = 5;
    public const int MaximumTicketsForHighPrice = 10;
    public const decimal HighPriceThreshold = 100m;
    public const int PenalizedCancellationWindowHours = 48;

    public static void EnsureCanCreate(
        EventSchedule eventSchedule,
        Money unitPrice,
        TicketQuantity quantity,
        DateTimeOffset now)
    {
        now = Guard.AgainstDefault(now, nameof(now));

        var timeToStart = eventSchedule.StartsAt - now;

        if (timeToStart <= TimeSpan.FromHours(MinimumHoursBeforeStart))
        {
            throw new DomainException("No se puede reservar a menos de 1 hora del inicio del evento.");
        }

        if (timeToStart <= TimeSpan.FromHours(NearEventWindowHours) &&
            quantity.Value > MaximumTicketsNearEvent)
        {
            throw new DomainException("Cuando faltan menos de 24 horas solo se pueden reservar maximo 5 entradas.");
        }

        if (unitPrice.Amount > HighPriceThreshold &&
            quantity.Value > MaximumTicketsForHighPrice)
        {
            throw new DomainException("Cuando el precio es mayor a 100 solo se pueden reservar maximo 10 entradas.");
        }
    }

    public static bool IsPenalizedCancellation(EventSchedule eventSchedule, DateTimeOffset cancelledAt)
    {
        cancelledAt = Guard.AgainstDefault(cancelledAt, nameof(cancelledAt));
        return eventSchedule.StartsAt - cancelledAt < TimeSpan.FromHours(PenalizedCancellationWindowHours);
    }
}
