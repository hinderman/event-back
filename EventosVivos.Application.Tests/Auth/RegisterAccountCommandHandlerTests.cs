using EventosVivos.Application.Auth.Register;
using EventosVivos.Application.Common.Errors;
using EventosVivos.Application.Tests.Common;
using EventosVivos.Domain.Catalogs;

namespace EventosVivos.Application.Tests.Auth;

public sealed class RegisterAccountCommandHandlerTests
{
    [Fact]
    public async Task Handle_CreatesUserAndIssuesSession()
    {
        var users = new FakeUserRepository();
        var catalogs = new FakeCatalogRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tokens = new FakeAuthTokenStore();
        var handler = new RegisterAccountCommandHandler(
            users,
            catalogs,
            unitOfWork,
            new FakePasswordHasher(),
            tokens,
            new TestDateTimeProvider(TestData.Now));

        var result = await handler.Handle(
            new RegisterAccountCommand("Nueva Compradora", "NUEVA@EVENTOS.COM", "Password123"),
            CancellationToken.None);

        Assert.Equal("Nueva Compradora", result.FullName);
        Assert.Equal("nueva@eventos.com", result.Email);
        Assert.Equal(UserType.BuyerCode, result.Role);
        Assert.Equal("token-1", result.Token);
        Assert.Single(users.Users);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.True(tokens.Sessions.ContainsKey(result.Token));
    }

    [Fact]
    public async Task Handle_WhenEmailExists_ThrowsConflict()
    {
        var users = new FakeUserRepository();
        users.Add(TestData.Buyer(email: "buyer@eventos.com"));
        var handler = new RegisterAccountCommandHandler(
            users,
            new FakeCatalogRepository(),
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new FakeAuthTokenStore(),
            new TestDateTimeProvider(TestData.Now));

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(
            new RegisterAccountCommand("Buyer User", "BUYER@EVENTOS.COM", "Password123"),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenPasswordIsTooShort_ThrowsApplicationRule()
    {
        var unitOfWork = new FakeUnitOfWork();
        var handler = new RegisterAccountCommandHandler(
            new FakeUserRepository(),
            new FakeCatalogRepository(),
            unitOfWork,
            new FakePasswordHasher(),
            new FakeAuthTokenStore(),
            new TestDateTimeProvider(TestData.Now));

        await Assert.ThrowsAsync<ApplicationRuleException>(() => handler.Handle(
            new RegisterAccountCommand("Buyer User", "buyer@eventos.com", "short"),
            CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }
}
