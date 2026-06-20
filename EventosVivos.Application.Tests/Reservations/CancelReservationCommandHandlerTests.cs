using EventosVivos.Application.Common.Errors;
using EventosVivos.Application.Reservations.CancelReservation;
using EventosVivos.Application.Tests.Common;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Application.Tests.Reservations;

public sealed class CancelReservationCommandHandlerTests
{
    [Fact]
    public async Task Handle_CancelsByBuyerEmailAndMarksPenaltyInsideFortyEightHours()
    {
        var users = new FakeUserRepository();
        users.Add(TestData.Buyer(email: "buyer@eventos.com"));
        var events = new FakeEventRepository();
        events.Add(TestData.ActiveEvent(startsAt: TestData.Now.AddHours(24)));
        var reservations = new FakeReservationRepository();
        reservations.Add(TestData.Reservation(statusId: 2));
        var unitOfWork = new FakeUnitOfWork();
        var handler = NewHandler(users, events, reservations, unitOfWork);

        var result = await handler.Handle(
            new CancelReservationCommand(30, null, "BUYER@EVENTOS.COM", "No puedo asistir"),
            CancellationToken.None);

        Assert.Equal(3, result.ReservationStatusId);
        Assert.Equal(2, result.CancelledByUserId);
        Assert.True(result.IsPenalizedCancellation);
        Assert.Equal("No puedo asistir", result.CancellationReason);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_CancelsByAdministratorWithoutPenaltyForPendingReservation()
    {
        var users = new FakeUserRepository();
        users.Add(TestData.Admin());
        var events = new FakeEventRepository();
        events.Add(TestData.ActiveEvent());
        var reservations = new FakeReservationRepository();
        reservations.Add(TestData.Reservation(statusId: 1));
        var handler = NewHandler(users, events, reservations, new FakeUnitOfWork());

        var result = await handler.Handle(
            new CancelReservationCommand(30, 1, null, null),
            CancellationToken.None);

        Assert.Equal(1, result.CancelledByUserId);
        Assert.False(result.IsPenalizedCancellation);
    }

    [Fact]
    public async Task Handle_WhenCancellingUserIsMissing_ThrowsApplicationRule()
    {
        var events = new FakeEventRepository();
        events.Add(TestData.ActiveEvent());
        var reservations = new FakeReservationRepository();
        reservations.Add(TestData.Reservation());
        var handler = NewHandler(new FakeUserRepository(), events, reservations, new FakeUnitOfWork());

        await Assert.ThrowsAsync<ApplicationRuleException>(() =>
            handler.Handle(new CancelReservationCommand(30, null, null, null), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenBuyerEmailDoesNotMatch_ThrowsForbidden()
    {
        var users = new FakeUserRepository();
        users.Add(TestData.Buyer(email: "buyer@eventos.com"));
        var events = new FakeEventRepository();
        events.Add(TestData.ActiveEvent());
        var reservations = new FakeReservationRepository();
        reservations.Add(TestData.Reservation());
        var handler = NewHandler(users, events, reservations, new FakeUnitOfWork());

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new CancelReservationCommand(30, null, "other@eventos.com", null), CancellationToken.None));
    }

    private static CancelReservationCommandHandler NewHandler(
        FakeUserRepository users,
        FakeEventRepository events,
        FakeReservationRepository reservations,
        FakeUnitOfWork unitOfWork)
    {
        var catalogs = new FakeCatalogRepository();
        return new CancelReservationCommandHandler(
            reservations,
            events,
            catalogs,
            users,
            new FakeUserAccessService(users, catalogs),
            unitOfWork,
            new TestDateTimeProvider(TestData.Now));
    }
}
