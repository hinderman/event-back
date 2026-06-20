using EventosVivos.Domain.Events;
using EventosVivos.Domain.Primitives;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Domain.Tests.Events;

public sealed class EventTests
{
    [Fact]
    public void Create_BuildsActiveEventWithEmptyId()
    {
        var now = new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero);
        var start = now.AddDays(3);

        var @event = Event.Create(
            new VenueId(1),
            new EventTypeId(1),
            new EventStatusId(1),
            new UserId(1),
            new EventText("Titulo valido", "Descripcion suficientemente larga"),
            new Capacity(100),
            new EventSchedule(start, start.AddHours(2)),
            new Money(80m),
            now);

        Assert.True(@event.Id.IsEmpty);
        Assert.Equal(new EventStatusId(1), @event.EventStatusId);
        Assert.Equal(now, @event.CreatedAt);
    }

    [Fact]
    public void Overlaps_ReturnsFalseForDifferentVenues()
    {
        var first = NewEvent(new VenueId(1));
        var second = NewEvent(new VenueId(2));

        Assert.False(first.Overlaps(second));
    }

    [Fact]
    public void ChangeStatus_UpdatesStatusAndTimestamp()
    {
        var @event = NewEvent(new VenueId(1));
        var updatedAt = new DateTimeOffset(2026, 6, 21, 12, 0, 0, TimeSpan.Zero);

        @event.ChangeStatus(new EventStatusId(3), updatedAt);

        Assert.Equal(new EventStatusId(3), @event.EventStatusId);
        Assert.Equal(updatedAt, @event.UpdatedAt);
    }

    [Fact]
    public void Constructor_RejectsInvalidVenue()
    {
        Assert.Throws<DomainException>(() => NewEvent(VenueId.Empty));
    }

    private static Event NewEvent(VenueId venueId)
    {
        var now = new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero);
        var start = now.AddDays(3);
        return new Event(
            new EventId(1),
            venueId,
            new EventTypeId(1),
            new EventStatusId(1),
            new UserId(1),
            new EventText("Titulo valido", "Descripcion suficientemente larga"),
            new Capacity(100),
            new EventSchedule(start, start.AddHours(2)),
            new Money(80m),
            now,
            now);
    }
}
