namespace EventosVivos.Api.Endpoints.Reservations.Requests;

public sealed record ConfirmReservationPaymentRequest(
    decimal PaidAmount,
    string? PaymentReference);
