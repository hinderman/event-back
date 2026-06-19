using EventosVivos.Application.Abstractions.Persistence;
using EventosVivos.Application.Common.Pagination;
using EventosVivos.Application.Events.ListEvents;
using EventosVivos.Domain.Catalogs;
using EventosVivos.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class EventReadRepository(EventosVivosDbContext dbContext) : IEventReadRepository
{
    public async Task<PagedResponse<EventSummaryResponse>> SearchAsync(
        EventSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query =
            from @event in dbContext.Events.AsNoTracking()
            join venue in dbContext.Venues.AsNoTracking()
                on @event.VenueId equals venue.Id
            join eventType in dbContext.EventTypes.AsNoTracking()
                on @event.EventTypeId equals eventType.Id
            join eventStatus in dbContext.EventStatuses.AsNoTracking()
                on @event.EventStatusId equals eventStatus.Id
            select new
            {
                Event = @event,
                Venue = venue,
                EventType = eventType,
                EventStatus = eventStatus
            };

        if (criteria.EventTypeCode.HasValue)
        {
            var eventTypeCode = criteria.EventTypeCode.Value;
            query = query.Where(item => item.EventType.Code == eventTypeCode);
        }

        if (criteria.StartsFrom.HasValue)
        {
            var start = new DateTimeOffset(criteria.StartsFrom.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            query = query.Where(item => item.Event.Schedule.StartsAt >= start);
        }

        if (criteria.StartsTo.HasValue)
        {
            var endExclusive = new DateTimeOffset(criteria.StartsTo.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
                .AddDays(1);
            query = query.Where(item => item.Event.Schedule.StartsAt < endExclusive);
        }

        if (criteria.VenueId.HasValue)
        {
            var venueId = criteria.VenueId.Value;
            query = query.Where(item => item.Event.VenueId == venueId);
        }

        if (criteria.EventStatusCode.HasValue)
        {
            var eventStatusCode = criteria.EventStatusCode.Value;
            query = query.Where(item => item.EventStatus.Code == eventStatusCode);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Title))
        {
            var titlePattern = $"%{criteria.Title}%";
            query = query.Where(item => EF.Functions.Like(item.Event.Details.Title, titlePattern));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderBy(item => item.Event.Schedule.StartsAt)
            .ThenBy(item => item.Event.Details.Title)
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(item => new EventSummaryRow(
                item.Event.Id,
                item.Event.Details.Title,
                item.Event.Details.Description,
                item.Venue.Id,
                item.Venue.Name,
                item.Venue.City,
                item.EventType.Code,
                item.EventType.Name,
                item.EventStatus.Code,
                item.EventStatus.Name,
                item.Event.MaxCapacity,
                item.Event.Schedule.StartsAt,
                item.Event.Schedule.EndsAt,
                item.Event.Price))
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return new PagedResponse<EventSummaryResponse>(
                Array.Empty<EventSummaryResponse>(),
                criteria.PageNumber,
                criteria.PageSize,
                totalItems);
        }

        var items = await ToResponsesAsync(rows, cancellationToken);

        return new PagedResponse<EventSummaryResponse>(
            items,
            criteria.PageNumber,
            criteria.PageSize,
            totalItems);
    }

    public async Task<EventSummaryResponse?> GetByIdAsync(
        long eventId,
        CancellationToken cancellationToken = default)
    {
        var id = new EventId(eventId);
        var rows = await (
            from @event in dbContext.Events.AsNoTracking()
            join venue in dbContext.Venues.AsNoTracking()
                on @event.VenueId equals venue.Id
            join eventType in dbContext.EventTypes.AsNoTracking()
                on @event.EventTypeId equals eventType.Id
            join eventStatus in dbContext.EventStatuses.AsNoTracking()
                on @event.EventStatusId equals eventStatus.Id
            where @event.Id == id
            select new
            {
                Event = @event,
                Venue = venue,
                EventType = eventType,
                EventStatus = eventStatus
            })
            .Select(item => new EventSummaryRow(
                item.Event.Id,
                item.Event.Details.Title,
                item.Event.Details.Description,
                item.Venue.Id,
                item.Venue.Name,
                item.Venue.City,
                item.EventType.Code,
                item.EventType.Name,
                item.EventStatus.Code,
                item.EventStatus.Name,
                item.Event.MaxCapacity,
                item.Event.Schedule.StartsAt,
                item.Event.Schedule.EndsAt,
                item.Event.Price))
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return null;
        }

        return (await ToResponsesAsync(rows, cancellationToken)).Single();
    }

    private async Task<IReadOnlyCollection<EventSummaryResponse>> ToResponsesAsync(
        IReadOnlyCollection<EventSummaryRow> rows,
        CancellationToken cancellationToken)
    {
        var reservedTicketsByEvent = await GetReservedTicketsByEventAsync(rows, cancellationToken);

        return rows
            .Select(row =>
            {
                var reservedTickets = reservedTicketsByEvent.GetValueOrDefault(row.Id);
                var availableTickets = Math.Max(row.MaxCapacity.Value - reservedTickets, 0);

                return new EventSummaryResponse(
                    row.Id.Value,
                    row.Title,
                    row.Description,
                    row.VenueId.Value,
                    row.VenueName,
                    row.City,
                    row.EventTypeCode.Value,
                    row.EventTypeName,
                    row.EventStatusCode.Value,
                    row.EventStatusName,
                    row.MaxCapacity.Value,
                    row.StartsAt,
                    row.EndsAt,
                    row.Price.Amount,
                    reservedTickets,
                    availableTickets);
            })
            .ToArray();
    }

    private async Task<IReadOnlyDictionary<EventId, int>> GetReservedTicketsByEventAsync(
        IReadOnlyCollection<EventSummaryRow> rows,
        CancellationToken cancellationToken)
    {
        var eventIds = rows.Select(row => row.Id).ToArray();
        var canceledStatusCode = new CatalogCode(ReservationStatus.CanceledCode);
        var canceledStatusId = await dbContext.ReservationStatuses
            .AsNoTracking()
            .Where(status => status.Code == canceledStatusCode)
            .Select(status => status.Id)
            .SingleAsync(cancellationToken);

        var reservations = await dbContext.Reservations
            .AsNoTracking()
            .Where(reservation =>
                eventIds.Contains(reservation.EventId) &&
                (reservation.ReservationStatusId != canceledStatusId ||
                    reservation.IsPenalizedCancellation))
            .ToListAsync(cancellationToken);

        return reservations
            .GroupBy(reservation => reservation.EventId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(reservation => reservation.Quantity.Value));
    }

    private sealed record EventSummaryRow(
        EventId Id,
        string Title,
        string Description,
        VenueId VenueId,
        string VenueName,
        string City,
        CatalogCode EventTypeCode,
        string EventTypeName,
        CatalogCode EventStatusCode,
        string EventStatusName,
        Capacity MaxCapacity,
        DateTimeOffset StartsAt,
        DateTimeOffset EndsAt,
        Money Price);

}
