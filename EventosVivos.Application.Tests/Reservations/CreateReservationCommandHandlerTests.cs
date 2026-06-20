using EventosVivos.Application.Common.Errors;
using EventosVivos.Application.Reservations.CreateReservation;
using EventosVivos.Application.Tests.Common;

namespace EventosVivos.Application.Tests.Reservations;

public sealed class CreateReservationCommandHandlerTests
{
    [Fact]
    public async Task Handle_CreatesReservation_WhenCapacityAndBuyerAreValid()
    {
        var users = new FakeUserRepository();
        users.Add(TestData.Buyer());
        var events = new FakeEventRepository();
        events.Add(TestData.ActiveEvent(capacity: 10));
        var reservations = new FakeReservationRepository { ReservedTickets = 8 };
        var unitOfWork = new FakeUnitOfWork();
        var handler = NewHandler(users, events, reservations, unitOfWork);

        var result = await handler.Handle(new CreateReservationCommand(20, 2, 2), CancellationToken.None);

        Assert.Equal(20, result.EventId);
        Assert.Equal(2, result.BuyerUserId);
        Assert.Equal(160m, result.TotalAmount);
        Assert.NotNull(reservations.AddedReservation);
        Assert.Equal(1, events.CompletePastActiveEventsCalls);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_WhenThereIsNotEnoughAvailability_ThrowsConflict()
    {
        var users = new FakeUserRepository();
        users.Add(TestData.Buyer());
        var events = new FakeEventRepository();
        events.Add(TestData.ActiveEvent(capacity: 10));
        var reservations = new FakeReservationRepository { ReservedTickets = 9 };
        var unitOfWork = new FakeUnitOfWork();
        var handler = NewHandler(users, events, reservations, unitOfWork);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CreateReservationCommand(20, 2, 2), CancellationToken.None));

        Assert.Null(reservations.AddedReservation);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_WhenEventIsNotActive_ThrowsConflict()
    {
        var users = new FakeUserRepository();
        users.Add(TestData.Buyer());
        var events = new FakeEventRepository();
        var inactiveEvent = TestData.ActiveEvent();
        inactiveEvent.ChangeStatus(new EventosVivos.Domain.ValueObjects.EventStatusId(3), TestData.Now);
        events.Add(inactiveEvent);
        var handler = NewHandler(users, events, new FakeReservationRepository(), new FakeUnitOfWork());

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CreateReservationCommand(20, 1, 2), CancellationToken.None));
    }

    private static CreateReservationCommandHandler NewHandler(
        FakeUserRepository users,
        FakeEventRepository events,
        FakeReservationRepository reservations,
        FakeUnitOfWork unitOfWork)
    {
        var catalogs = new FakeCatalogRepository();
        return new CreateReservationCommandHandler(
            events,
            reservations,
            catalogs,
            new FakeUserAccessService(users, catalogs),
            unitOfWork,
            new TestDateTimeProvider(TestData.Now));
    }
}
