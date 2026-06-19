namespace EventosVivos.Application.Reservations.ListReservations;

public sealed record ReservationListItemResponse(
    long Id,
    long EventId,
    string EventTitle,
    long BuyerUserId,
    string BuyerName,
    string BuyerEmail,
    short ReservationStatusId,
    string ReservationStatusCode,
    string ReservationStatusName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalAmount,
    string? ReservationCode,
    bool IsPenalizedCancellation,
    string? CancellationReason,
    long? CancelledByUserId,
    DateTimeOffset? CancelledAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    ReservationPaymentResponse? Payment);
