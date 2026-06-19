using EventosVivos.Application.Abstractions.Clock;
using EventosVivos.Application.Abstractions.Messaging;
using EventosVivos.Application.Abstractions.Persistence;
using EventosVivos.Application.Common.Errors;
using EventosVivos.Application.Common.Security;
using EventosVivos.Domain.Catalogs;
using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Application.Reservations.CreateReservation;

public sealed class CreateReservationCommandHandler(
    IEventRepository eventRepository,
    IReservationRepository reservationRepository,
    ICatalogRepository catalogRepository,
    IUserAccessService userAccessService,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateReservationCommand, ReservationResponse>
{
    public async Task<ReservationResponse> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var eventId = new EventId(request.EventId);
        var buyerUserId = new UserId(request.BuyerUserId);
        var now = dateTimeProvider.UtcNow;
        await CompletePastEventsAsync(now, cancellationToken);

        var @event = await eventRepository.GetByIdAsync(eventId, cancellationToken);

        if (@event is null)
        {
            throw new NotFoundException("Evento", eventId.Value);
        }

        var activeEventStatus = await GetEventStatusAsync(EventStatus.ActiveCode, cancellationToken);

        if (@event.EventStatusId != activeEventStatus.Id)
        {
            throw new ConflictException("Solo se pueden crear reservas para eventos activos.");
        }

        var pendingStatus = await GetReservationStatusAsync(ReservationStatus.PendingPaymentCode, cancellationToken);
        var canceledStatus = await GetReservationStatusAsync(ReservationStatus.CanceledCode, cancellationToken);
        var quantity = new TicketQuantity(request.Quantity);
        var reservedTickets = await reservationRepository.CountReservedTicketsByEventIdAsync(
            @event.Id,
            canceledStatus.Id,
            cancellationToken);

        if (@event.MaxCapacity.Value - reservedTickets < quantity.Value)
        {
            throw new ConflictException("No hay disponibilidad suficiente para la cantidad solicitada.");
        }

        ReservationPolicy.EnsureCanCreate(@event.Schedule, @event.Price, quantity, now);
        var buyer = await userAccessService.EnsureActiveBuyerAsync(buyerUserId, cancellationToken);

        var reservation = Reservation.Create(
            @event.Id,
            buyer.Id,
            pendingStatus.Id,
            quantity,
            @event.Price,
            @event.Schedule,
            now);

        await reservationRepository.AddAsync(reservation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ReservationResponse.FromReservation(reservation);
    }

    private async Task<EventStatus> GetEventStatusAsync(string code, CancellationToken cancellationToken)
    {
        var statusCode = new CatalogCode(code);
        var status = await catalogRepository.GetEventStatusByCodeAsync(statusCode, cancellationToken);

        return status ?? throw new ApplicationRuleException($"El estado de evento '{code}' no esta configurado.");
    }

    private async Task CompletePastEventsAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var activeStatus = await GetEventStatusAsync(EventStatus.ActiveCode, cancellationToken);
        var completedStatus = await GetEventStatusAsync(EventStatus.CompletedCode, cancellationToken);

        await eventRepository.CompletePastActiveEventsAsync(
            activeStatus.Id,
            completedStatus.Id,
            now,
            cancellationToken);
    }

    private async Task<ReservationStatus> GetReservationStatusAsync(string code, CancellationToken cancellationToken)
    {
        var statusCode = new CatalogCode(code);
        var status = await catalogRepository.GetReservationStatusByCodeAsync(statusCode, cancellationToken);

        return status ?? throw new ApplicationRuleException($"El estado de reserva '{code}' no esta configurado.");
    }
}
