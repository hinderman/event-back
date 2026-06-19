using EventosVivos.Application.Abstractions.Messaging;
using EventosVivos.Application.Reservations;

namespace EventosVivos.Application.Reservations.ConfirmReservationPayment;

public sealed record ConfirmReservationPaymentCommand(
    long ReservationId,
    long ConfirmedByUserId,
    decimal PaidAmount,
    string? PaymentReference) : ICommand<ReservationResponse>;
