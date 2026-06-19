using EventosVivos.Application.Abstractions.Clock;
using EventosVivos.Application.Abstractions.Messaging;
using EventosVivos.Application.Abstractions.Persistence;
using EventosVivos.Application.Abstractions.Reservations;
using EventosVivos.Application.Common.Errors;
using EventosVivos.Application.Common.Security;
using EventosVivos.Domain.Catalogs;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Application.Reservations.ConfirmReservationPayment;

public sealed class ConfirmReservationPaymentCommandHandler(
    IReservationRepository reservationRepository,
    ICatalogRepository catalogRepository,
    IUserAccessService userAccessService,
    IReservationCodeGenerator reservationCodeGenerator,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<ConfirmReservationPaymentCommand, ReservationResponse>
{
    public async Task<ReservationResponse> Handle(
        ConfirmReservationPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var reservationId = new ReservationId(request.ReservationId);
        var reservation = await reservationRepository.GetByIdAsync(reservationId, cancellationToken);

        if (reservation is null)
        {
            throw new NotFoundException("Reserva", reservationId.Value);
        }

        var administrator = await userAccessService.EnsureActiveAdministratorAsync(
            new UserId(request.ConfirmedByUserId),
            cancellationToken);

        var confirmedStatus = await GetReservationStatusAsync(ReservationStatus.ConfirmedCode, cancellationToken);
        var pendingStatus = await GetReservationStatusAsync(ReservationStatus.PendingPaymentCode, cancellationToken);
        var canceledStatus = await GetReservationStatusAsync(ReservationStatus.CanceledCode, cancellationToken);

        if (reservation.ReservationStatusId == confirmedStatus.Id)
        {
            throw new ConflictException("La reserva ya esta confirmada.");
        }

        if (reservation.ReservationStatusId == canceledStatus.Id)
        {
            throw new ConflictException("No se puede confirmar una reserva cancelada.");
        }

        if (reservation.ReservationStatusId != pendingStatus.Id)
        {
            throw new ConflictException("Solo se pueden confirmar reservas pendientes de pago.");
        }

        var reservationCode = await reservationCodeGenerator.GenerateAsync(cancellationToken);

        reservation.ConfirmPayment(
            confirmedStatus.Id,
            reservationCode,
            administrator.Id,
            new Money(request.PaidAmount),
            request.PaymentReference,
            dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ReservationResponse.FromReservation(reservation);
    }

    private async Task<ReservationStatus> GetReservationStatusAsync(string code, CancellationToken cancellationToken)
    {
        var statusCode = new CatalogCode(code);
        var status = await catalogRepository.GetReservationStatusByCodeAsync(statusCode, cancellationToken);

        return status ?? throw new ApplicationRuleException($"El estado de reserva '{code}' no esta configurado.");
    }
}
