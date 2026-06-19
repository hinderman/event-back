using EventosVivos.Application.Abstractions.Persistence;
using EventosVivos.Domain.Catalogs;
using EventosVivos.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class CatalogRepository(EventosVivosDbContext dbContext) : ICatalogRepository
{
    public Task<UserType?> GetUserTypeByIdAsync(
        UserTypeId userTypeId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.UserTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(userType => userType.Id == userTypeId, cancellationToken);
    }

    public Task<UserType?> GetUserTypeByCodeAsync(
        CatalogCode code,
        CancellationToken cancellationToken = default)
    {
        return dbContext.UserTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(userType => userType.Code == code, cancellationToken);
    }

    public Task<EventType?> GetEventTypeByCodeAsync(
        CatalogCode code,
        CancellationToken cancellationToken = default)
    {
        return dbContext.EventTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(eventType => eventType.Code == code, cancellationToken);
    }

    public Task<EventStatus?> GetEventStatusByCodeAsync(
        CatalogCode code,
        CancellationToken cancellationToken = default)
    {
        return dbContext.EventStatuses
            .AsNoTracking()
            .SingleOrDefaultAsync(eventStatus => eventStatus.Code == code, cancellationToken);
    }

    public Task<ReservationStatus?> GetReservationStatusByCodeAsync(
        CatalogCode code,
        CancellationToken cancellationToken = default)
    {
        return dbContext.ReservationStatuses
            .AsNoTracking()
            .SingleOrDefaultAsync(reservationStatus => reservationStatus.Code == code, cancellationToken);
    }
}
