using EventosVivos.Domain.Primitives;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Domain.Reservations;

public sealed class Reservation : AggregateRoot<ReservationId>
{
    private Reservation()
    {
        Quantity = default;
        UnitPrice = default;
    }

    public Reservation(
        ReservationId id,
        EventId eventId,
        UserId buyerUserId,
        ReservationStatusId reservationStatusId,
        TicketQuantity quantity,
        Money unitPrice,
        ReservationCode? reservationCode,
        Money? paidAmount,
        string? paymentReference,
        DateTimeOffset? confirmedAt,
        UserId? confirmedByUserId,
        bool isPenalizedCancellation,
        string? cancellationReason,
        UserId? cancelledByUserId,
        DateTimeOffset? cancelledAt,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
        : base(id)
    {
        Guard.AgainstNonPositive(eventId.Value, nameof(eventId));
        Guard.AgainstNonPositive(buyerUserId.Value, nameof(buyerUserId));
        Guard.AgainstNonPositive(reservationStatusId.Value, nameof(reservationStatusId));
        EnsurePaymentMetadata(paidAmount, paymentReference, confirmedAt, confirmedByUserId);
        EnsureCancellationMetadata(cancelledByUserId, cancelledAt, isPenalizedCancellation);

        EventId = eventId;
        BuyerUserId = buyerUserId;
        ReservationStatusId = reservationStatusId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        ReservationCode = reservationCode;
        PaidAmount = paidAmount;
        PaymentReference = NormalizeOptionalText(paymentReference, 80, nameof(paymentReference));
        ConfirmedAt = confirmedAt;
        ConfirmedByUserId = confirmedByUserId;
        IsPenalizedCancellation = isPenalizedCancellation;
        CancellationReason = NormalizeOptionalText(cancellationReason, 300, nameof(cancellationReason));
        CancelledByUserId = cancelledByUserId;
        CancelledAt = cancelledAt;
        CreatedAt = Guard.AgainstDefault(createdAt, nameof(createdAt));
        UpdatedAt = Guard.AgainstDefault(updatedAt, nameof(updatedAt));
    }

    public EventId EventId { get; private set; }

    public UserId BuyerUserId { get; private set; }

    public ReservationStatusId ReservationStatusId { get; private set; }

    public TicketQuantity Quantity { get; private set; }

    public Money UnitPrice { get; private set; }

    public ReservationCode? ReservationCode { get; private set; }

    public Money? PaidAmount { get; private set; }

    public string? PaymentReference { get; private set; }

    public DateTimeOffset? ConfirmedAt { get; private set; }

    public UserId? ConfirmedByUserId { get; private set; }

    public bool IsPenalizedCancellation { get; private set; }

    public string? CancellationReason { get; private set; }

    public UserId? CancelledByUserId { get; private set; }

    public DateTimeOffset? CancelledAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public Money TotalAmount => UnitPrice.Multiply(Quantity.Value);

    public static Reservation Create(
        EventId eventId,
        UserId buyerUserId,
        ReservationStatusId pendingPaymentStatusId,
        TicketQuantity quantity,
        Money unitPrice,
        EventSchedule eventSchedule,
        DateTimeOffset now)
    {
        ReservationPolicy.EnsureCanCreate(eventSchedule, unitPrice, quantity, now);

        return new Reservation(
            ReservationId.Empty,
            eventId,
            buyerUserId,
            pendingPaymentStatusId,
            quantity,
            unitPrice,
            null,
            null,
            null,
            null,
            null,
            false,
            null,
            null,
            null,
            now,
            now);
    }

    public void ConfirmPayment(
        ReservationStatusId confirmedStatusId,
        ReservationCode reservationCode,
        UserId confirmedByUserId,
        Money paidAmount,
        string? paymentReference,
        DateTimeOffset confirmedAt)
    {
        Guard.AgainstNonPositive(confirmedStatusId.Value, nameof(confirmedStatusId));
        Guard.AgainstNonPositive(confirmedByUserId.Value, nameof(confirmedByUserId));

        if (Id.IsEmpty)
        {
            throw new DomainException("La reserva debe estar persistida antes de confirmar el pago.");
        }

        if (CancelledAt.HasValue)
        {
            throw new DomainException("No se puede confirmar el pago de una reserva cancelada.");
        }

        if (ReservationCode.HasValue || PaidAmount.HasValue || ConfirmedAt.HasValue || ConfirmedByUserId.HasValue)
        {
            throw new DomainException("La reserva ya tiene un pago confirmado.");
        }

        if (paidAmount != TotalAmount)
        {
            throw new DomainException("El valor pagado debe coincidir con el total de la reserva.");
        }

        ReservationStatusId = confirmedStatusId;
        ReservationCode = reservationCode;
        PaidAmount = paidAmount;
        PaymentReference = NormalizeOptionalText(paymentReference, 80, nameof(paymentReference));
        ConfirmedAt = Guard.AgainstDefault(confirmedAt, nameof(confirmedAt));
        ConfirmedByUserId = confirmedByUserId;
        Touch(confirmedAt);
    }

    public void Cancel(
        ReservationStatusId canceledStatusId,
        UserId cancelledByUserId,
        string? reason,
        bool isPenalizedCancellation,
        DateTimeOffset cancelledAt)
    {
        Guard.AgainstNonPositive(canceledStatusId.Value, nameof(canceledStatusId));
        Guard.AgainstNonPositive(cancelledByUserId.Value, nameof(cancelledByUserId));

        if (CancelledAt.HasValue)
        {
            throw new DomainException("La reserva ya esta cancelada.");
        }

        ReservationStatusId = canceledStatusId;
        CancelledByUserId = cancelledByUserId;
        CancellationReason = NormalizeOptionalText(reason, 300, nameof(reason));
        IsPenalizedCancellation = isPenalizedCancellation;
        CancelledAt = Guard.AgainstDefault(cancelledAt, nameof(cancelledAt));
        Touch(cancelledAt);
    }

    private void Touch(DateTimeOffset updatedAt)
    {
        UpdatedAt = Guard.AgainstDefault(updatedAt, nameof(updatedAt));
    }

    private static void EnsurePaymentMetadata(
        Money? paidAmount,
        string? paymentReference,
        DateTimeOffset? confirmedAt,
        UserId? confirmedByUserId)
    {
        if (confirmedByUserId.HasValue && confirmedByUserId.Value.IsEmpty)
        {
            throw new DomainException("El usuario que confirma el pago debe ser valido.");
        }

        if (confirmedAt.HasValue)
        {
            Guard.AgainstDefault(confirmedAt.Value, nameof(confirmedAt));
        }

        if (paidAmount.HasValue != confirmedAt.HasValue ||
            paidAmount.HasValue != confirmedByUserId.HasValue)
        {
            throw new DomainException("La confirmacion de pago debe tener valor, usuario y fecha al mismo tiempo.");
        }

        NormalizeOptionalText(paymentReference, 80, nameof(paymentReference));
    }

    private static string? NormalizeOptionalText(string? value, int maxLength, string parameterName)
    {
        if (value is null)
        {
            return null;
        }

        value = Guard.AgainstBlank(value, parameterName);
        return Guard.AgainstLength(value, maxLength, parameterName);
    }

    private static void EnsureCancellationMetadata(
        UserId? cancelledByUserId,
        DateTimeOffset? cancelledAt,
        bool isPenalizedCancellation)
    {
        if (cancelledByUserId.HasValue && cancelledByUserId.Value.IsEmpty)
        {
            throw new DomainException("El usuario que cancela la reserva debe ser valido.");
        }

        if (cancelledAt.HasValue)
        {
            Guard.AgainstDefault(cancelledAt.Value, nameof(cancelledAt));
        }

        if (cancelledByUserId.HasValue != cancelledAt.HasValue)
        {
            throw new DomainException("La cancelacion debe tener usuario y fecha al mismo tiempo.");
        }

        if (isPenalizedCancellation && !cancelledAt.HasValue)
        {
            throw new DomainException("Una cancelacion penalizada debe tener fecha de cancelacion.");
        }
    }
}
