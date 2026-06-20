using EventosVivos.Application.Common.Errors;
using EventosVivos.Application.Reservations.ConfirmReservationPayment;
using EventosVivos.Application.Tests.Common;

namespace EventosVivos.Application.Tests.Reservations;

public sealed class ConfirmReservationPaymentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ConfirmsPendingReservationAndAssignsCode()
    {
        var users = new FakeUserRepository();
        users.Add(TestData.Admin());
        var reservations = new FakeReservationRepository();
        reservations.Add(TestData.Reservation());
        var unitOfWork = new FakeUnitOfWork();
        var handler = NewHandler(users, reservations, unitOfWork);

        var result = await handler.Handle(
            new ConfirmReservationPaymentCommand(30, 1, 160m, "PAY-001"),
            CancellationToken.None);

        Assert.Equal(2, result.ReservationStatusId);
        Assert.Equal("EV-000123", result.ReservationCode);
        Assert.Equal(160m, result.Payment?.PaidAmount);
        Assert.Equal("PAY-001", result.Payment?.PaymentReference);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_WhenReservationIsAlreadyConfirmed_ThrowsConflict()
    {
        var users = new FakeUserRepository();
        users.Add(TestData.Admin());
        var reservations = new FakeReservationRepository();
        var reservation = TestData.Reservation(statusId: 2);
        reservations.Add(reservation);
        var unitOfWork = new FakeUnitOfWork();
        var handler = NewHandler(users, reservations, unitOfWork);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(
            new ConfirmReservationPaymentCommand(30, 1, 160m, null),
            CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_WhenPaidAmountDoesNotMatchTotal_ThrowsDomainRule()
    {
        var users = new FakeUserRepository();
        users.Add(TestData.Admin());
        var reservations = new FakeReservationRepository();
        reservations.Add(TestData.Reservation());
        var handler = NewHandler(users, reservations, new FakeUnitOfWork());

        await Assert.ThrowsAsync<EventosVivos.Domain.Primitives.DomainException>(() => handler.Handle(
            new ConfirmReservationPaymentCommand(30, 1, 159m, null),
            CancellationToken.None));
    }

    private static ConfirmReservationPaymentCommandHandler NewHandler(
        FakeUserRepository users,
        FakeReservationRepository reservations,
        FakeUnitOfWork unitOfWork)
    {
        var catalogs = new FakeCatalogRepository();
        return new ConfirmReservationPaymentCommandHandler(
            reservations,
            catalogs,
            new FakeUserAccessService(users, catalogs),
            new FakeReservationCodeGenerator(),
            unitOfWork,
            new TestDateTimeProvider(TestData.Now));
    }
}
