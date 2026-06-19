using EventosVivos.Domain.Reservations;

namespace EventosVivos.Application.Reservations;

public sealed record ReservationResponse(
    long Id,
    long EventId,
    long BuyerUserId,
    short ReservationStatusId,
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
    ReservationPaymentResponse? Payment)
{
    public static ReservationResponse FromReservation(Reservation reservation)
    {
        return new ReservationResponse(
            reservation.Id.Value,
            reservation.EventId.Value,
            reservation.BuyerUserId.Value,
            reservation.ReservationStatusId.Value,
            reservation.Quantity.Value,
            reservation.UnitPrice.Amount,
            reservation.TotalAmount.Amount,
            reservation.ReservationCode?.Value,
            reservation.IsPenalizedCancellation,
            reservation.CancellationReason,
            reservation.CancelledByUserId?.Value,
            reservation.CancelledAt,
            reservation.CreatedAt,
            reservation.UpdatedAt,
            ReservationPaymentResponse.FromReservation(reservation));
    }
}

public sealed record ReservationPaymentResponse(
    long ReservationId,
    long ConfirmedByUserId,
    decimal PaidAmount,
    string? PaymentReference,
    DateTimeOffset ConfirmedAt)
{
    public static ReservationPaymentResponse? FromReservation(Reservation reservation)
    {
        if (!reservation.PaidAmount.HasValue ||
            !reservation.ConfirmedByUserId.HasValue ||
            !reservation.ConfirmedAt.HasValue)
        {
            return null;
        }

        return new ReservationPaymentResponse(
            reservation.Id.Value,
            reservation.ConfirmedByUserId.Value.Value,
            reservation.PaidAmount.Value.Amount,
            reservation.PaymentReference,
            reservation.ConfirmedAt.Value);
    }
}
