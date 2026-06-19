using EventosVivos.Application.Events.CreateEvent;
using EventosVivos.Application.Events.GetEventById;
using EventosVivos.Application.Events.ListEvents;
using EventosVivos.Application.Reports;
using MediatR;

namespace EventosVivos.Api.Endpoints;

public static class EventEndpoints
{
    public static IEndpointRouteBuilder MapEventEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/events")
            .WithTags("Events");

        group.MapGet("/", async (
            ISender sender,
            string? eventTypeCode,
            DateOnly? date,
            DateOnly? startsFrom,
            DateOnly? startsTo,
            int? venueId,
            string? eventStatusCode,
            string? title,
            int? pageNumber,
            int? pageSize,
            CancellationToken cancellationToken) =>
        {
            startsFrom ??= date;
            startsTo ??= date;

            var response = await sender.Send(
                new ListEventsQuery(
                    eventTypeCode,
                    startsFrom,
                    startsTo,
                    venueId,
                    eventStatusCode,
                    title,
                    pageNumber.GetValueOrDefault(1),
                    pageSize.GetValueOrDefault(10)),
                cancellationToken);

            return Results.Ok(response);
        });

        group.MapGet("/{eventId:long}", async (
            long eventId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var response = await sender.Send(new GetEventByIdQuery(eventId), cancellationToken);
            return Results.Ok(response);
        });

        group.MapPost("/", async (
            CreateEventRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var response = await sender.Send(
                new CreateEventCommand(
                    request.VenueId,
                    request.EventTypeCode,
                    request.CreatedByUserId,
                    request.Title,
                    request.Description,
                    request.MaxCapacity,
                    request.StartsAt,
                    request.EndsAt,
                    request.Price),
                cancellationToken);

            return Results.Created($"/api/events/{response.Id}", response);
        });

        group.MapGet("/{eventId:long}/occupancy", async (
            long eventId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var response = await sender.Send(new GetEventOccupancyReportQuery(eventId), cancellationToken);
            return Results.Ok(response);
        });

        return endpoints;
    }
}

public sealed record CreateEventRequest(
    int VenueId,
    string EventTypeCode,
    long CreatedByUserId,
    string Title,
    string Description,
    int MaxCapacity,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    decimal Price);
