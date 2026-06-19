using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Application.Abstractions.Reservations;

public interface IReservationCodeGenerator
{
    Task<ReservationCode> GenerateAsync(CancellationToken cancellationToken = default);
}
