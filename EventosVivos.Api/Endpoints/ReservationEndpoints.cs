using EventosVivos.Application.Reservations.CancelReservation;
using EventosVivos.Application.Reservations.ConfirmReservationPayment;
using EventosVivos.Application.Reservations.CreateReservation;
using EventosVivos.Application.Reservations.ListReservations;
using MediatR;

namespace EventosVivos.Api.Endpoints;

public static class ReservationEndpoints
{
    public static IEndpointRouteBuilder MapReservationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/reservations")
            .WithTags("Reservations");

        group.MapGet("/", async (
            ISender sender,
            int? pageNumber,
            int? pageSize,
            string? sortBy,
            string? sortDirection,
            CancellationToken cancellationToken) =>
        {
            var response = await sender.Send(
                new ListReservationsQuery(
                    pageNumber ?? 1,
                    pageSize ?? 10,
                    sortBy ?? "createdAt",
                    sortDirection ?? "desc"),
                cancellationToken);

            return Results.Ok(response);
        });

        group.MapPost("/", async (
            CreateReservationRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var response = await sender.Send(
                new CreateReservationCommand(
                    request.EventId,
                    request.Quantity,
                    request.BuyerUserId),
                cancellationToken);

            return Results.Created($"/api/reservations/{response.Id}", response);
        });

        group.MapPost("/{reservationId:long}/confirm-payment", async (
            long reservationId,
            ConfirmReservationPaymentRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var response = await sender.Send(
                new ConfirmReservationPaymentCommand(
                    reservationId,
                    request.ConfirmedByUserId,
                    request.PaidAmount,
                    request.PaymentReference),
                cancellationToken);

            return Results.Ok(response);
        });

        group.MapPost("/{reservationId:long}/cancel", async (
            long reservationId,
            CancelReservationRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var response = await sender.Send(
                new CancelReservationCommand(
                    reservationId,
                    request.CancelledByUserId,
                    request.CancelledByEmail,
                    request.Reason),
                cancellationToken);

            return Results.Ok(response);
        });

        return endpoints;
    }
}

public sealed record CreateReservationRequest(
    long EventId,
    int Quantity,
    long BuyerUserId);

public sealed record ConfirmReservationPaymentRequest(
    long ConfirmedByUserId,
    decimal PaidAmount,
    string? PaymentReference);

public sealed record CancelReservationRequest(
    long? CancelledByUserId,
    string? CancelledByEmail,
    string? Reason);
