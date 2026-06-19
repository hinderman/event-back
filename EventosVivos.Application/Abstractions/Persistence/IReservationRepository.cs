using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Application.Abstractions.Persistence;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(ReservationId reservationId, CancellationToken cancellationToken = default);

    Task<int> CountReservedTicketsByEventIdAsync(
        EventId eventId,
        ReservationStatusId canceledStatusId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
}
