namespace EventosVivos.Api.Endpoints.Reservations.Requests;

public sealed record CreateReservationRequest(
    long EventId,
    int Quantity);
