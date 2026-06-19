using EventosVivos.Application.Abstractions.Persistence;
using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class ReservationRepository(EventosVivosDbContext dbContext) : IReservationRepository
{
    public Task<Reservation?> GetByIdAsync(
        ReservationId reservationId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Reservations
            .SingleOrDefaultAsync(reservation => reservation.Id == reservationId, cancellationToken);
    }

    public async Task<int> CountReservedTicketsByEventIdAsync(
        EventId eventId,
        ReservationStatusId canceledStatusId,
        CancellationToken cancellationToken = default)
    {
        var reservations = await dbContext.Reservations
            .AsNoTracking()
            .Where(reservation =>
                reservation.EventId == eventId &&
                (reservation.ReservationStatusId != canceledStatusId ||
                    reservation.IsPenalizedCancellation))
            .ToListAsync(cancellationToken);

        return reservations.Sum(reservation => reservation.Quantity.Value);
    }

    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        await dbContext.Reservations.AddAsync(reservation, cancellationToken);
    }
}
