using EventosVivos.Application.Abstractions.Persistence;
using EventosVivos.Application.Reports;
using EventosVivos.Domain.Catalogs;
using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class ReportingRepository(EventosVivosDbContext dbContext) : IReportingRepository
{
    public async Task<IReadOnlyCollection<EventOccupancyReportResponse>> GetEventOccupancyAsync(
        EventId? eventId = null,
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

        if (eventId.HasValue)
        {
            var requestedEventId = eventId.Value;
            query = query.Where(item => item.Event.Id == requestedEventId);
        }

        var eventRows = await query
            .OrderBy(item => item.Event.Schedule.StartsAt)
            .Select(item => new EventReportRow(
                item.Event.Id,
                item.Event.Details.Title,
                item.EventType.Code,
                item.EventStatus.Code,
                item.Venue.Id,
                item.Venue.Name,
                item.Venue.City,
                item.Event.MaxCapacity))
            .ToListAsync(cancellationToken);

        if (eventRows.Count == 0)
        {
            return Array.Empty<EventOccupancyReportResponse>();
        }

        var eventIds = eventRows.Select(row => row.EventId).ToArray();
        var reservations = await dbContext.Reservations
            .AsNoTracking()
            .Where(reservation => eventIds.Contains(reservation.EventId))
            .ToListAsync(cancellationToken);

        var canceledStatusId = await GetReservationStatusIdAsync(
            ReservationStatus.CanceledCode,
            cancellationToken);
        var confirmedStatusId = await GetReservationStatusIdAsync(
            ReservationStatus.ConfirmedCode,
            cancellationToken);

        return eventRows
            .Select(row => BuildResponse(row, reservations, canceledStatusId, confirmedStatusId))
            .ToArray();
    }

    private async Task<ReservationStatusId> GetReservationStatusIdAsync(
        string code,
        CancellationToken cancellationToken)
    {
        var statusCode = new CatalogCode(code);

        return await dbContext.ReservationStatuses
            .AsNoTracking()
            .Where(status => status.Code == statusCode)
            .Select(status => status.Id)
            .SingleAsync(cancellationToken);
    }

    private static EventOccupancyReportResponse BuildResponse(
        EventReportRow row,
        IReadOnlyCollection<Reservation> reservations,
        ReservationStatusId canceledStatusId,
        ReservationStatusId confirmedStatusId)
    {
        var eventReservations = reservations
            .Where(reservation => reservation.EventId == row.EventId)
            .ToArray();
        var unavailableReservations = eventReservations
            .Where(reservation =>
                reservation.ReservationStatusId != canceledStatusId ||
                reservation.IsPenalizedCancellation)
            .ToArray();
        var confirmedReservations = eventReservations
            .Where(reservation => reservation.ReservationStatusId == confirmedStatusId)
            .ToArray();

        var unavailableTickets = unavailableReservations.Sum(reservation => reservation.Quantity.Value);
        var confirmedTickets = confirmedReservations.Sum(reservation => reservation.Quantity.Value);
        var confirmedRevenue = confirmedReservations.Sum(reservation =>
            reservation.PaidAmount?.Amount ?? 0m);
        var projectedRevenue = unavailableReservations.Sum(reservation => reservation.TotalAmount.Amount);
        var availableTickets = Math.Max(row.MaxCapacity.Value - unavailableTickets, 0);
        var occupancyPercentage = row.MaxCapacity.Value == 0
            ? 0m
            : Math.Round(
                (decimal)confirmedTickets / row.MaxCapacity.Value * 100m,
                2,
                MidpointRounding.AwayFromZero);

        return new EventOccupancyReportResponse(
            row.EventId.Value,
            row.Title,
            row.EventTypeCode.Value,
            row.EventStatusCode.Value,
            row.VenueId.Value,
            row.VenueName,
            row.City,
            row.MaxCapacity.Value,
            unavailableTickets,
            confirmedTickets,
            availableTickets,
            occupancyPercentage,
            confirmedRevenue,
            projectedRevenue);
    }

    private sealed record EventReportRow(
        EventId EventId,
        string Title,
        CatalogCode EventTypeCode,
        CatalogCode EventStatusCode,
        VenueId VenueId,
        string VenueName,
        string City,
        Capacity MaxCapacity);
}
