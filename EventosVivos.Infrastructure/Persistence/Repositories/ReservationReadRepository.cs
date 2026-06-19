using EventosVivos.Application.Abstractions.Persistence;
using EventosVivos.Application.Common.Pagination;
using EventosVivos.Application.Reservations;
using EventosVivos.Application.Reservations.ListReservations;
using EventosVivos.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class ReservationReadRepository(EventosVivosDbContext dbContext) : IReservationReadRepository
{
    public async Task<PagedResponse<ReservationListItemResponse>> SearchAsync(
        ReservationSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query =
            from reservation in dbContext.Reservations.AsNoTracking()
            join @event in dbContext.Events.AsNoTracking()
                on reservation.EventId equals @event.Id
            join buyer in dbContext.AppUsers.AsNoTracking()
                on reservation.BuyerUserId equals buyer.Id
            join reservationStatus in dbContext.ReservationStatuses.AsNoTracking()
                on reservation.ReservationStatusId equals reservationStatus.Id
            select new
            {
                Reservation = reservation,
                Event = @event,
                Buyer = buyer,
                ReservationStatus = reservationStatus
            };

        var totalItems = await query.CountAsync(cancellationToken);
        var descending = criteria.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
        var orderedQuery = criteria.SortBy.ToLowerInvariant() switch
        {
            "updatedat" => descending
                ? query.OrderByDescending(item => item.Reservation.UpdatedAt)
                    .ThenByDescending(item => item.Reservation.Id)
                : query.OrderBy(item => item.Reservation.UpdatedAt)
                    .ThenBy(item => item.Reservation.Id),
            "eventtitle" => descending
                ? query.OrderByDescending(item => item.Event.Details.Title)
                    .ThenByDescending(item => item.Reservation.CreatedAt)
                : query.OrderBy(item => item.Event.Details.Title)
                    .ThenByDescending(item => item.Reservation.CreatedAt),
            "buyername" => descending
                ? query.OrderByDescending(item => item.Buyer.FullName)
                    .ThenByDescending(item => item.Reservation.CreatedAt)
                : query.OrderBy(item => item.Buyer.FullName)
                    .ThenByDescending(item => item.Reservation.CreatedAt),
            "status" => descending
                ? query.OrderByDescending(item => item.ReservationStatus.Code)
                    .ThenByDescending(item => item.Reservation.CreatedAt)
                : query.OrderBy(item => item.ReservationStatus.Code)
                    .ThenByDescending(item => item.Reservation.CreatedAt),
            "totalamount" => descending
                ? query.OrderByDescending(item => item.Reservation.UnitPrice.Amount * item.Reservation.Quantity.Value)
                    .ThenByDescending(item => item.Reservation.CreatedAt)
                : query.OrderBy(item => item.Reservation.UnitPrice.Amount * item.Reservation.Quantity.Value)
                    .ThenByDescending(item => item.Reservation.CreatedAt),
            _ => descending
                ? query.OrderByDescending(item => item.Reservation.CreatedAt)
                    .ThenByDescending(item => item.Reservation.Id)
                : query.OrderBy(item => item.Reservation.CreatedAt)
                    .ThenBy(item => item.Reservation.Id)
        };

        var rows = await orderedQuery
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(item => new ReservationListRow(
                item.Reservation.Id,
                item.Reservation.EventId,
                item.Event.Details.Title,
                item.Reservation.BuyerUserId,
                item.Buyer.FullName,
                item.Buyer.Email,
                item.Reservation.ReservationStatusId,
                item.ReservationStatus.Code,
                item.ReservationStatus.Name,
                item.Reservation.Quantity,
                item.Reservation.UnitPrice,
                item.Reservation.ReservationCode,
                item.Reservation.IsPenalizedCancellation,
                item.Reservation.CancellationReason,
                item.Reservation.CancelledByUserId,
                item.Reservation.CancelledAt,
                item.Reservation.CreatedAt,
                item.Reservation.UpdatedAt,
                item.Reservation.PaidAmount,
                item.Reservation.PaymentReference,
                item.Reservation.ConfirmedAt,
                item.Reservation.ConfirmedByUserId))
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new ReservationListItemResponse(
                row.Id.Value,
                row.EventId.Value,
                row.EventTitle,
                row.BuyerUserId.Value,
                row.BuyerName,
                row.BuyerEmail.Value,
                row.ReservationStatusId.Value,
                row.ReservationStatusCode.Value,
                row.ReservationStatusName,
                row.Quantity.Value,
                row.UnitPrice.Amount,
                row.UnitPrice.Multiply(row.Quantity.Value).Amount,
                row.ReservationCode?.Value,
                row.IsPenalizedCancellation,
                row.CancellationReason,
                row.CancelledByUserId?.Value,
                row.CancelledAt,
                row.CreatedAt,
                row.UpdatedAt,
                BuildPaymentResponse(row)))
            .ToArray();

        return new PagedResponse<ReservationListItemResponse>(
            items,
            criteria.PageNumber,
            criteria.PageSize,
            totalItems);
    }

    private static ReservationPaymentResponse? BuildPaymentResponse(ReservationListRow row)
    {
        if (!row.PaidAmount.HasValue ||
            !row.ConfirmedByUserId.HasValue ||
            !row.ConfirmedAt.HasValue)
        {
            return null;
        }

        return new ReservationPaymentResponse(
            row.Id.Value,
            row.ConfirmedByUserId.Value.Value,
            row.PaidAmount.Value.Amount,
            row.PaymentReference,
            row.ConfirmedAt.Value);
    }

    private sealed record ReservationListRow(
        ReservationId Id,
        EventId EventId,
        string EventTitle,
        UserId BuyerUserId,
        string BuyerName,
        Email BuyerEmail,
        ReservationStatusId ReservationStatusId,
        CatalogCode ReservationStatusCode,
        string ReservationStatusName,
        TicketQuantity Quantity,
        Money UnitPrice,
        ReservationCode? ReservationCode,
        bool IsPenalizedCancellation,
        string? CancellationReason,
        UserId? CancelledByUserId,
        DateTimeOffset? CancelledAt,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        Money? PaidAmount,
        string? PaymentReference,
        DateTimeOffset? ConfirmedAt,
        UserId? ConfirmedByUserId);
}
