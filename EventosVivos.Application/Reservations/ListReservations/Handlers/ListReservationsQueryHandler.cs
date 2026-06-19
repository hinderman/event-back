using EventosVivos.Application.Abstractions.Messaging;
using EventosVivos.Application.Abstractions.Persistence;
using EventosVivos.Application.Common.Errors;
using EventosVivos.Application.Common.Pagination;

namespace EventosVivos.Application.Reservations.ListReservations;

public sealed class ListReservationsQueryHandler(IReservationReadRepository reservationReadRepository)
    : IQueryHandler<ListReservationsQuery, PagedResponse<ReservationListItemResponse>>
{
    private static readonly string[] AllowedSortFields =
    [
        "createdAt",
        "updatedAt",
        "eventTitle",
        "buyerName",
        "status",
        "totalAmount"
    ];

    public async Task<PagedResponse<ReservationListItemResponse>> Handle(
        ListReservationsQuery request,
        CancellationToken cancellationToken)
    {
        var criteria = new ReservationSearchCriteria(
            NormalizePageNumber(request.PageNumber),
            NormalizePageSize(request.PageSize),
            NormalizeSortBy(request.SortBy),
            NormalizeSortDirection(request.SortDirection));

        return await reservationReadRepository.SearchAsync(criteria, cancellationToken);
    }

    private static int NormalizePageNumber(int pageNumber)
    {
        if (pageNumber <= 0)
        {
            throw new ApplicationRuleException("El numero de pagina debe ser mayor que cero.");
        }

        return pageNumber;
    }

    private static int NormalizePageSize(int pageSize)
    {
        if (pageSize is <= 0 or > 50)
        {
            throw new ApplicationRuleException("El tamano de pagina debe estar entre 1 y 50.");
        }

        return pageSize;
    }

    private static string NormalizeSortBy(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return "createdAt";
        }

        sortBy = sortBy.Trim();

        return AllowedSortFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase)
            ? sortBy
            : throw new ApplicationRuleException("El campo de ordenamiento de reservas no es valido.");
    }

    private static string NormalizeSortDirection(string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortDirection))
        {
            return "desc";
        }

        sortDirection = sortDirection.Trim().ToLowerInvariant();

        return sortDirection is "asc" or "desc"
            ? sortDirection
            : throw new ApplicationRuleException("La direccion de ordenamiento debe ser 'asc' o 'desc'.");
    }
}
