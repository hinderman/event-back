using EventosVivos.Application.Auth.Login;
using EventosVivos.Application.Common.Errors;
using EventosVivos.Application.Tests.Common;

namespace EventosVivos.Application.Tests.Auth;

public sealed class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsSession()
    {
        var users = new FakeUserRepository();
        users.Add(TestData.Admin());
        var tokens = new FakeAuthTokenStore();
        var handler = new LoginCommandHandler(
            users,
            new FakeCatalogRepository(),
            new FakePasswordHasher(),
            tokens,
            new TestDateTimeProvider(TestData.Now));

        var result = await handler.Handle(new LoginCommand("ADMIN@EVENTOS.COM", "Password123"), CancellationToken.None);

        Assert.Equal(1, result.UserId);
        Assert.Equal("administrador", result.Role);
        Assert.Equal("token-1", result.Token);
        Assert.Equal(TestData.Now, tokens.Sessions[result.Token].IssuedAt);
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ThrowsUnauthorized()
    {
        var users = new FakeUserRepository();
        users.Add(TestData.Admin());
        var handler = new LoginCommandHandler(
            users,
            new FakeCatalogRepository(),
            new FakePasswordHasher(),
            new FakeAuthTokenStore(),
            new TestDateTimeProvider(TestData.Now));

        await Assert.ThrowsAsync<AuthUnauthorizedException>(() =>
            handler.Handle(new LoginCommand("admin@eventos.com", "wrong-password"), CancellationToken.None));
    }
}
