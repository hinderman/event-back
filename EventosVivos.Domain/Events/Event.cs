using EventosVivos.Domain.Primitives;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Domain.Events;

public sealed class Event : AggregateRoot<EventId>
{
    private Event()
    {
        Details = default;
        Schedule = default;
        Price = default;
    }

    public Event(
        EventId id,
        VenueId venueId,
        EventTypeId eventTypeId,
        EventStatusId eventStatusId,
        UserId createdByUserId,
        EventText details,
        Capacity maxCapacity,
        EventSchedule schedule,
        Money price,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
        : base(id)
    {
        Guard.AgainstNonPositive(venueId.Value, nameof(venueId));
        Guard.AgainstNonPositive(eventTypeId.Value, nameof(eventTypeId));
        Guard.AgainstNonPositive(eventStatusId.Value, nameof(eventStatusId));
        Guard.AgainstNonPositive(createdByUserId.Value, nameof(createdByUserId));

        VenueId = venueId;
        EventTypeId = eventTypeId;
        EventStatusId = eventStatusId;
        CreatedByUserId = createdByUserId;
        Details = details;
        MaxCapacity = maxCapacity;
        Schedule = schedule;
        Price = price;
        CreatedAt = Guard.AgainstDefault(createdAt, nameof(createdAt));
        UpdatedAt = Guard.AgainstDefault(updatedAt, nameof(updatedAt));
    }

    public VenueId VenueId { get; private set; }

    public EventTypeId EventTypeId { get; private set; }

    public EventStatusId EventStatusId { get; private set; }

    public UserId CreatedByUserId { get; private set; }

    public EventText Details { get; private set; }

    public Capacity MaxCapacity { get; private set; }

    public EventSchedule Schedule { get; private set; }

    public Money Price { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Event Create(
        VenueId venueId,
        EventTypeId eventTypeId,
        EventStatusId activeStatusId,
        UserId createdByUserId,
        EventText details,
        Capacity maxCapacity,
        EventSchedule schedule,
        Money price,
        DateTimeOffset now)
    {
        EventCreationPolicy.EnsureCanCreate(schedule, now);

        return new Event(
            EventId.Empty,
            venueId,
            eventTypeId,
            activeStatusId,
            createdByUserId,
            details,
            maxCapacity,
            schedule,
            price,
            now,
            now);
    }

    public bool Overlaps(Event other)
    {
        return VenueId == other.VenueId && Schedule.Overlaps(other.Schedule);
    }

    public void ChangeStatus(EventStatusId statusId, DateTimeOffset updatedAt)
    {
        Guard.AgainstNonPositive(statusId.Value, nameof(statusId));

        EventStatusId = statusId;
        Touch(updatedAt);
    }

    private void Touch(DateTimeOffset updatedAt)
    {
        UpdatedAt = Guard.AgainstDefault(updatedAt, nameof(updatedAt));
    }
}
