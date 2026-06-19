namespace EventosVivos.Application.Events.ListEvents;

public sealed record EventSummaryResponse(
    long Id,
    string Title,
    string Description,
    int VenueId,
    string VenueName,
    string City,
    string EventTypeCode,
    string EventTypeName,
    string EventStatusCode,
    string EventStatusName,
    int MaxCapacity,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    decimal Price,
    int ReservedTickets,
    int AvailableTickets);
