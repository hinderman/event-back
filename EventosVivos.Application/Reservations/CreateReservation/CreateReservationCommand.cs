using EventosVivos.Application.Abstractions.Messaging;
using EventosVivos.Application.Reservations;

namespace EventosVivos.Application.Reservations.CreateReservation;

public sealed record CreateReservationCommand(
    long EventId,
    int Quantity,
    long BuyerUserId) : ICommand<ReservationResponse>;
