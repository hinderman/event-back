using EventosVivos.Application.Abstractions.Messaging;
using EventosVivos.Application.Events;

namespace EventosVivos.Application.Events.CreateEvent;

public sealed record CreateEventCommand(
    int VenueId,
    string EventTypeCode,
    long CreatedByUserId,
    string Title,
    string Description,
    int MaxCapacity,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    decimal Price) : ICommand<EventResponse>;
