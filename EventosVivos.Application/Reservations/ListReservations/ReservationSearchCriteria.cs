namespace EventosVivos.Application.Reservations.ListReservations;

public sealed record ReservationSearchCriteria(
    int PageNumber,
    int PageSize,
    string SortBy,
    string SortDirection);
