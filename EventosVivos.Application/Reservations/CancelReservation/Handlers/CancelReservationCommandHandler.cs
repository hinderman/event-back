using EventosVivos.Application.Abstractions.Clock;
using EventosVivos.Application.Abstractions.Messaging;
using EventosVivos.Application.Abstractions.Persistence;
using EventosVivos.Application.Common.Errors;
using EventosVivos.Application.Common.Security;
using EventosVivos.Domain.Catalogs;
using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Application.Reservations.CancelReservation;

public sealed class CancelReservationCommandHandler(
    IReservationRepository reservationRepository,
    IEventRepository eventRepository,
    ICatalogRepository catalogRepository,
    IUserRepository userRepository,
    IUserAccessService userAccessService,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CancelReservationCommand, ReservationResponse>
{
    public async Task<ReservationResponse> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        var reservationId = new ReservationId(request.ReservationId);
        var reservation = await reservationRepository.GetByIdAsync(reservationId, cancellationToken);

        if (reservation is null)
        {
            throw new NotFoundException("Reserva", reservationId.Value);
        }

        var @event = await eventRepository.GetByIdAsync(reservation.EventId, cancellationToken);

        if (@event is null)
        {
            throw new NotFoundException("Evento", reservation.EventId.Value);
        }

        var cancelledByUserId = await ResolveCancellingUserIdAsync(request, reservation, cancellationToken);
        var canceledStatus = await GetReservationStatusAsync(ReservationStatus.CanceledCode, cancellationToken);
        var confirmedStatus = await GetReservationStatusAsync(ReservationStatus.ConfirmedCode, cancellationToken);

        if (reservation.ReservationStatusId == canceledStatus.Id)
        {
            throw new ConflictException("La reserva ya esta cancelada.");
        }

        var cancelledAt = dateTimeProvider.UtcNow;
        var isPenalized = reservation.ReservationStatusId == confirmedStatus.Id &&
            ReservationPolicy.IsPenalizedCancellation(@event.Schedule, cancelledAt);

        reservation.Cancel(
            canceledStatus.Id,
            cancelledByUserId,
            request.Reason,
            isPenalized,
            cancelledAt);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ReservationResponse.FromReservation(reservation);
    }

    private async Task<UserId> ResolveCancellingUserIdAsync(
        CancelReservationCommand request,
        Reservation reservation,
        CancellationToken cancellationToken)
    {
        if (request.CancelledByUserId.HasValue)
        {
            var cancelledBy = await userAccessService.EnsureActiveUserAsync(
                new UserId(request.CancelledByUserId.Value),
                cancellationToken);

            await userAccessService.EnsureCanCancelReservationAsync(cancelledBy, reservation, cancellationToken);
            return cancelledBy.Id;
        }

        if (!string.IsNullOrWhiteSpace(request.CancelledByEmail))
        {
            var email = new Email(request.CancelledByEmail);
            var buyer = await userRepository.GetByIdAsync(reservation.BuyerUserId, cancellationToken);

            if (buyer is null)
            {
                throw new NotFoundException("Comprador", reservation.BuyerUserId.Value);
            }

            if (!buyer.IsActive)
            {
                throw new ForbiddenAccessException("El comprador no esta activo.");
            }

            if (buyer.Email != email)
            {
                throw new ForbiddenAccessException("Solo el comprador de la reserva o un administrador puede cancelarla.");
            }

            return buyer.Id;
        }

        throw new ApplicationRuleException("Debe indicar el usuario administrador o el correo del comprador que cancela.");
    }

    private async Task<ReservationStatus> GetReservationStatusAsync(string code, CancellationToken cancellationToken)
    {
        var statusCode = new CatalogCode(code);
        var status = await catalogRepository.GetReservationStatusByCodeAsync(statusCode, cancellationToken);

        return status ?? throw new ApplicationRuleException($"El estado de reserva '{code}' no esta configurado.");
    }
}
