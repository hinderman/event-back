using EventosVivos.Application.Common.Pagination;
using EventosVivos.Application.Reservations.ListReservations;

namespace EventosVivos.Application.Abstractions.Persistence;

public interface IReservationReadRepository
{
    Task<PagedResponse<ReservationListItemResponse>> SearchAsync(
        ReservationSearchCriteria criteria,
        CancellationToken cancellationToken = default);
}
