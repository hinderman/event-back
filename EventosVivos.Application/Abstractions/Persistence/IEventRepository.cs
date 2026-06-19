using EventosVivos.Domain.Events;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Application.Abstractions.Persistence;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(EventId eventId, CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveOverlapAsync(
        VenueId venueId,
        EventSchedule schedule,
        EventStatusId activeStatusId,
        CancellationToken cancellationToken = default);

    Task<int> CompletePastActiveEventsAsync(
        EventStatusId activeStatusId,
        EventStatusId completedStatusId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);

    Task AddAsync(Event @event, CancellationToken cancellationToken = default);
}
