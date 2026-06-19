using EventosVivos.Domain.Events;

namespace EventosVivos.Application.Events;

public sealed record EventResponse(
    long Id,
    int VenueId,
    short EventTypeId,
    short EventStatusId,
    long CreatedByUserId,
    string Title,
    string Description,
    int MaxCapacity,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    decimal Price,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static EventResponse FromEvent(Event @event)
    {
        return new EventResponse(
            @event.Id.Value,
            @event.VenueId.Value,
            @event.EventTypeId.Value,
            @event.EventStatusId.Value,
            @event.CreatedByUserId.Value,
            @event.Details.Title,
            @event.Details.Description,
            @event.MaxCapacity.Value,
            @event.Schedule.StartsAt,
            @event.Schedule.EndsAt,
            @event.Price.Amount,
            @event.CreatedAt,
            @event.UpdatedAt);
    }
}
