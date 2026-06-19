using EventosVivos.Application.Abstractions.Reservations;
using EventosVivos.Domain.ValueObjects;
using EventosVivos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Reservations;

internal sealed class ReservationCodeGenerator(EventosVivosDbContext dbContext) : IReservationCodeGenerator
{
    public async Task<ReservationCode> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var lastSequence = await dbContext.Database
            .SqlQueryRaw<long>("""
                SELECT COALESCE(MAX(CAST(SUBSTRING(reservation_code, 4, 6) AS bigint)), 0) AS [Value]
                FROM eventos_vivos.reservations
                WHERE reservation_code IS NOT NULL
                """)
            .SingleAsync(cancellationToken);

        return ReservationCode.FromSequence(lastSequence + 1);
    }
}
