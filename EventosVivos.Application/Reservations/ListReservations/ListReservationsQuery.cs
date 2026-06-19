using EventosVivos.Application.Abstractions.Messaging;
using EventosVivos.Application.Common.Pagination;

namespace EventosVivos.Application.Reservations.ListReservations;

public sealed record ListReservationsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string SortBy = "createdAt",
    string SortDirection = "desc") : IQuery<PagedResponse<ReservationListItemResponse>>;
