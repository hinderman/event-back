using EventosVivos.Application.Common.Errors;
using EventosVivos.Application.Events.CreateEvent;
using EventosVivos.Application.Tests.Common;

namespace EventosVivos.Application.Tests.Events;

public sealed class CreateEventCommandHandlerTests
{
    [Fact]
    public async Task Handle_CreatesEvent_WhenRequestIsValid()
    {
        var users = new FakeUserRepository();
        users.Add(TestData.Admin());
        var venues = new FakeVenueRepository();
        venues.Add(TestData.Venue());
        var events = new FakeEventRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = NewHandler(users, venues, events, unitOfWork);

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        Assert.Equal(10, result.VenueId);
        Assert.Equal(1, result.EventStatusId);
        Assert.Equal("Conferencia Clean", result.Title);
        Assert.Same(events.AddedEvent, events.AddedEvent);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_WhenVenueCapacityIsInsufficient_ThrowsConflict()
    {
        var users = new FakeUserRepository();
        users.Add(TestData.Admin());
        var venues = new FakeVenueRepository();
        venues.Add(TestData.Venue(capacity: 10));
        var unitOfWork = new FakeUnitOfWork();
        var handler = NewHandler(users, venues, new FakeEventRepository(), unitOfWork);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(ValidCommand(maxCapacity: 11), CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_WhenVenueHasOverlap_ThrowsConflict()
    {
        var users = new FakeUserRepository();
        users.Add(TestData.Admin());
        var venues = new FakeVenueRepository();
        venues.Add(TestData.Venue());
        var events = new FakeEventRepository { HasActiveOverlap = true };
        var unitOfWork = new FakeUnitOfWork();
        var handler = NewHandler(users, venues, events, unitOfWork);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(ValidCommand(), CancellationToken.None));

        Assert.NotNull(events.LastOverlapSchedule);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }

    private static CreateEventCommandHandler NewHandler(
        FakeUserRepository users,
        FakeVenueRepository venues,
        FakeEventRepository events,
        FakeUnitOfWork unitOfWork)
    {
        var catalogs = new FakeCatalogRepository();
        return new CreateEventCommandHandler(
            events,
            venues,
            catalogs,
            new FakeUserAccessService(users, catalogs),
            unitOfWork,
            new TestDateTimeProvider(TestData.Now));
    }

    private static CreateEventCommand ValidCommand(int maxCapacity = 100)
    {
        return new CreateEventCommand(
            10,
            "conferencia",
            1,
            "Conferencia Clean",
            "Aprendizajes de arquitectura limpia",
            maxCapacity,
            TestData.Now.AddDays(5),
            TestData.Now.AddDays(5).AddHours(2),
            80m);
    }
}
