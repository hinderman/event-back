using EventosVivos.Domain.Events;
using EventosVivos.Domain.Primitives;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Domain.Tests.Events;

public sealed class EventCreationPolicyTests
{
    [Fact]
    public void EnsureCanCreate_RejectsPastStart()
    {
        var now = new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero);
        var schedule = new EventSchedule(now.AddMinutes(-30), now.AddHours(2));

        Assert.Throws<DomainException>(() => EventCreationPolicy.EnsureCanCreate(schedule, now));
    }

    [Fact]
    public void EnsureCanCreate_RejectsWeekendStartAtOrAfterTenPm()
    {
        var now = new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero);
        var saturdayNight = new DateTimeOffset(2026, 6, 20, 22, 0, 0, TimeSpan.Zero);
        var schedule = new EventSchedule(saturdayNight, saturdayNight.AddHours(2));

        Assert.Throws<DomainException>(() => EventCreationPolicy.EnsureCanCreate(schedule, now));
    }

    [Fact]
    public void EnsureCanCreate_AllowsFutureWeekdayEvent()
    {
        var now = new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero);
        var monday = new DateTimeOffset(2026, 6, 22, 20, 0, 0, TimeSpan.Zero);
        var schedule = new EventSchedule(monday, monday.AddHours(2));

        EventCreationPolicy.EnsureCanCreate(schedule, now);
    }
}
