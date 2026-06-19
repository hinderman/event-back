using EventosVivos.Domain.Primitives;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Domain.Venues;

public sealed class Venue : AggregateRoot<VenueId>
{
    private Venue()
    {
        Name = string.Empty;
        City = string.Empty;
    }

    public Venue(
        VenueId id,
        string name,
        string city,
        Capacity capacity,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
        : base(id)
    {
        Name = Guard.AgainstLength(Guard.AgainstBlank(name, nameof(name)), 150, nameof(name));
        City = Guard.AgainstLength(Guard.AgainstBlank(city, nameof(city)), 100, nameof(city));
        Capacity = capacity;
        IsActive = isActive;
        CreatedAt = Guard.AgainstDefault(createdAt, nameof(createdAt));
        UpdatedAt = Guard.AgainstDefault(updatedAt, nameof(updatedAt));
    }

    public string Name { get; private set; }

    public string City { get; private set; }

    public Capacity Capacity { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Venue Create(string name, string city, Capacity capacity, DateTimeOffset now)
    {
        return new Venue(VenueId.Empty, name, city, capacity, true, now, now);
    }

    public void Activate(DateTimeOffset updatedAt)
    {
        IsActive = true;
        Touch(updatedAt);
    }

    public void Deactivate(DateTimeOffset updatedAt)
    {
        IsActive = false;
        Touch(updatedAt);
    }

    public bool CanHost(Capacity eventCapacity)
    {
        return Capacity.Value >= eventCapacity.Value;
    }

    private void Touch(DateTimeOffset updatedAt)
    {
        UpdatedAt = Guard.AgainstDefault(updatedAt, nameof(updatedAt));
    }
}
