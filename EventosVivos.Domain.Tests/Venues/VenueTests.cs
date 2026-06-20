using EventosVivos.Domain.Primitives;
using EventosVivos.Domain.ValueObjects;
using EventosVivos.Domain.Venues;

namespace EventosVivos.Domain.Tests.Venues;

public sealed class VenueTests
{
    [Fact]
    public void Create_BuildsActiveVenue()
    {
        var now = new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero);

        var venue = Venue.Create("Auditorio", "Bogota", new Capacity(100), now);

        Assert.True(venue.Id.IsEmpty);
        Assert.True(venue.IsActive);
        Assert.Equal("Auditorio", venue.Name);
    }

    [Theory]
    [InlineData(100, 100, true)]
    [InlineData(100, 101, false)]
    public void CanHost_ReturnsExpectedResult(int venueCapacity, int eventCapacity, bool expected)
    {
        var venue = Venue.Create(
            "Auditorio",
            "Bogota",
            new Capacity(venueCapacity),
            new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero));

        Assert.Equal(expected, venue.CanHost(new Capacity(eventCapacity)));
    }

    [Fact]
    public void Deactivate_ChangesStateAndTimestamp()
    {
        var now = new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero);
        var venue = Venue.Create("Auditorio", "Bogota", new Capacity(100), now);
        var updatedAt = now.AddHours(1);

        venue.Deactivate(updatedAt);

        Assert.False(venue.IsActive);
        Assert.Equal(updatedAt, venue.UpdatedAt);
    }

    [Fact]
    public void Constructor_RejectsBlankName()
    {
        var now = new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero);

        Assert.Throws<DomainException>(() => Venue.Create(" ", "Bogota", new Capacity(100), now));
    }
}
