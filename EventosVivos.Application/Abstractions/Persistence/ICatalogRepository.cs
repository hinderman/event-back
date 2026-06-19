using EventosVivos.Domain.Catalogs;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Application.Abstractions.Persistence;

public interface ICatalogRepository
{
    Task<UserType?> GetUserTypeByIdAsync(UserTypeId userTypeId, CancellationToken cancellationToken = default);

    Task<UserType?> GetUserTypeByCodeAsync(CatalogCode code, CancellationToken cancellationToken = default);

    Task<EventType?> GetEventTypeByCodeAsync(CatalogCode code, CancellationToken cancellationToken = default);

    Task<EventStatus?> GetEventStatusByCodeAsync(CatalogCode code, CancellationToken cancellationToken = default);

    Task<ReservationStatus?> GetReservationStatusByCodeAsync(CatalogCode code, CancellationToken cancellationToken = default);
}
