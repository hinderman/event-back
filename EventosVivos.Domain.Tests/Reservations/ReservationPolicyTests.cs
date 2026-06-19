using EventosVivos.Domain.Primitives;
using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Domain.Tests.Reservations;

public sealed class ReservationPolicyTests
{
    [Fact]
    public void EnsureCanCreate_RejectsReservationsWithLessThanOneHourBeforeStart()
    {
        var now = new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero);
        var schedule = new EventSchedule(now.AddMinutes(30), now.AddHours(2));

        Assert.Throws<DomainException>(() =>
            ReservationPolicy.EnsureCanCreate(schedule, new Money(80m), new TicketQuantity(1), now));
    }

    [Fact]
    public void EnsureCanCreate_RejectsMoreThanFiveTicketsInsideTwentyFourHourWindow()
    {
        var now = new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero);
        var schedule = new EventSchedule(now.AddHours(3), now.AddHours(6));

        Assert.Throws<DomainException>(() =>
            ReservationPolicy.EnsureCanCreate(schedule, new Money(80m), new TicketQuantity(6), now));
    }

    [Fact]
    public void EnsureCanCreate_RejectsMoreThanTenTicketsForHighPriceEvents()
    {
        var now = new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero);
        var schedule = new EventSchedule(now.AddDays(5), now.AddDays(5).AddHours(2));

        Assert.Throws<DomainException>(() =>
            ReservationPolicy.EnsureCanCreate(schedule, new Money(120m), new TicketQuantity(11), now));
    }

    [Fact]
    public void IsPenalizedCancellation_ReturnsTrueInsideFortyEightHourWindow()
    {
        var startsAt = new DateTimeOffset(2026, 6, 21, 12, 0, 0, TimeSpan.Zero);
        var schedule = new EventSchedule(startsAt, startsAt.AddHours(2));

        var result = ReservationPolicy.IsPenalizedCancellation(schedule, startsAt.AddHours(-47));

        Assert.True(result);
    }

    [Fact]
    public void IsPenalizedCancellation_ReturnsFalseOutsideFortyEightHourWindow()
    {
        var startsAt = new DateTimeOffset(2026, 6, 21, 12, 0, 0, TimeSpan.Zero);
        var schedule = new EventSchedule(startsAt, startsAt.AddHours(2));

        var result = ReservationPolicy.IsPenalizedCancellation(schedule, startsAt.AddHours(-49));

        Assert.False(result);
    }
}
