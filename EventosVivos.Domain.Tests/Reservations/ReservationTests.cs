using EventosVivos.Domain.Primitives;
using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Domain.Tests.Reservations;

public sealed class ReservationTests
{
    [Fact]
    public void ConfirmPayment_AssignsCodeAndPaymentMetadata()
    {
        var now = new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero);
        var reservation = NewPersistedReservation(now);

        reservation.ConfirmPayment(
            new ReservationStatusId(2),
            new ReservationCode("EV-000123"),
            new UserId(1),
            new Money(160m),
            "PAY-001",
            now.AddMinutes(5));

        Assert.Equal(new ReservationStatusId(2), reservation.ReservationStatusId);
        Assert.Equal("EV-000123", reservation.ReservationCode?.Value);
        Assert.Equal(160m, reservation.PaidAmount?.Amount);
        Assert.Equal(new UserId(1), reservation.ConfirmedByUserId);
    }

    [Fact]
    public void ConfirmPayment_RejectsDifferentPaidAmount()
    {
        var now = new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero);
        var reservation = NewPersistedReservation(now);

        Assert.Throws<DomainException>(() =>
            reservation.ConfirmPayment(
                new ReservationStatusId(2),
                new ReservationCode("EV-000123"),
                new UserId(1),
                new Money(159m),
                null,
                now.AddMinutes(5)));
    }

    [Fact]
    public void Cancel_StoresCancellationMetadata()
    {
        var now = new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero);
        var reservation = NewPersistedReservation(now);

        reservation.Cancel(new ReservationStatusId(3), new UserId(8), "Cambio de planes", true, now.AddMinutes(10));

        Assert.Equal(new ReservationStatusId(3), reservation.ReservationStatusId);
        Assert.Equal(new UserId(8), reservation.CancelledByUserId);
        Assert.True(reservation.IsPenalizedCancellation);
        Assert.NotNull(reservation.CancelledAt);
    }

    private static Reservation NewPersistedReservation(DateTimeOffset now)
    {
        return new Reservation(
            new ReservationId(10),
            new EventId(20),
            new UserId(30),
            new ReservationStatusId(1),
            new TicketQuantity(2),
            new Money(80m),
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
}
