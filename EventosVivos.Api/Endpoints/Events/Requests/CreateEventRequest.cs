namespace EventosVivos.Api.Endpoints.Events.Requests;

public sealed record CreateEventRequest(
    int VenueId,
    string EventTypeCode,
    string Title,
    string Description,
    int MaxCapacity,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    decimal Price);
