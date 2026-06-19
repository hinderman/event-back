using EventosVivos.Application.Abstractions.Messaging;
using EventosVivos.Application.Reservations;

namespace EventosVivos.Application.Reservations.CancelReservation;

public sealed record CancelReservationCommand(
    long ReservationId,
    long? CancelledByUserId,
    string? CancelledByEmail,
    string? Reason) : ICommand<ReservationResponse>;
