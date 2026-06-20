using System.Security.Claims;
using EventosVivos.Api.Endpoints.Reservations.Requests;
using EventosVivos.Api.Infrastructure;
using EventosVivos.Application.Reservations.CancelReservation;
using EventosVivos.Application.Reservations.ConfirmReservationPayment;
using EventosVivos.Application.Reservations.CreateReservation;
using EventosVivos.Application.Reservations.ListReservations;
using MediatR;

namespace EventosVivos.Api.Endpoints.Reservations;

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
        }).RequireAuthorization(AuthPolicies.Administrator);

        group.MapPost("/", async (
            CreateReservationRequest request,
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var response = await sender.Send(
                new CreateReservationCommand(
                    request.EventId,
                    request.Quantity,
                    user.GetUserId()),
                cancellationToken);

            return Results.Created($"/api/reservations/{response.Id}", response);
        }).RequireAuthorization(AuthPolicies.Buyer);

        group.MapPost("/{reservationId:long}/confirm-payment", async (
            long reservationId,
            ConfirmReservationPaymentRequest request,
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var response = await sender.Send(
                new ConfirmReservationPaymentCommand(
                    reservationId,
                    user.GetUserId(),
                    request.PaidAmount,
                    request.PaymentReference),
                cancellationToken);

            return Results.Ok(response);
        }).RequireAuthorization(AuthPolicies.Administrator);

        group.MapPost("/{reservationId:long}/cancel", async (
            long reservationId,
            CancelReservationRequest request,
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var response = await sender.Send(
                new CancelReservationCommand(
                    reservationId,
                    user.GetUserId(),
                    null,
                    request.Reason),
                cancellationToken);

            return Results.Ok(response);
        }).RequireAuthorization(AuthPolicies.AdministratorOrBuyer);

        return endpoints;
    }
}
