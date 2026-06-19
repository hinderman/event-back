using EventosVivos.Application.Abstractions.Persistence;
using EventosVivos.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using DomainEvent = EventosVivos.Domain.Events.Event;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class EventRepository(EventosVivosDbContext dbContext) : IEventRepository
{
    public Task<DomainEvent?> GetByIdAsync(EventId eventId, CancellationToken cancellationToken = default)
    {
        return dbContext.Events
            .SingleOrDefaultAsync(@event => @event.Id == eventId, cancellationToken);
    }

    public Task<bool> ExistsActiveOverlapAsync(
        VenueId venueId,
        EventSchedule schedule,
        EventStatusId activeStatusId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Events
            .AsNoTracking()
            .AnyAsync(
                @event =>
                    @event.VenueId == venueId &&
                    @event.EventStatusId == activeStatusId &&
                    @event.Schedule.StartsAt < schedule.EndsAt &&
                    @event.Schedule.EndsAt > schedule.StartsAt,
                cancellationToken);
    }

    public Task<int> CompletePastActiveEventsAsync(
        EventStatusId activeStatusId,
        EventStatusId completedStatusId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Events
            .Where(@event =>
                @event.EventStatusId == activeStatusId &&
                @event.Schedule.EndsAt < now)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(@event => @event.EventStatusId, completedStatusId)
                    .SetProperty(@event => @event.UpdatedAt, now),
                cancellationToken);
    }

    public async Task AddAsync(DomainEvent @event, CancellationToken cancellationToken = default)
    {
        await dbContext.Events.AddAsync(@event, cancellationToken);
    }
}
