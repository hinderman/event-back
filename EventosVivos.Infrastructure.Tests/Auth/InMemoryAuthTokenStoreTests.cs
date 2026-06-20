using EventosVivos.Application.Abstractions.Auth;
using EventosVivos.Domain.ValueObjects;
using EventosVivos.Infrastructure.Auth;

namespace EventosVivos.Infrastructure.Tests.Auth;

public sealed class InMemoryAuthTokenStoreTests
{
    [Fact]
    public void IssueToken_StoresSessionAndReturnsUrlSafeToken()
    {
        var store = new InMemoryAuthTokenStore();
        var session = NewSession();

        var token = store.IssueToken(session);

        Assert.DoesNotContain("+", token);
        Assert.DoesNotContain("/", token);
        Assert.DoesNotContain("=", token);
        Assert.True(store.TryGetSession(token, out var storedSession));
        Assert.Equal(session, storedSession);
    }

    [Fact]
    public void IssueToken_ReturnsDifferentTokenForEachSession()
    {
        var store = new InMemoryAuthTokenStore();

        var first = store.IssueToken(NewSession());
        var second = store.IssueToken(NewSession());

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void RevokeToken_RemovesExistingSession()
    {
        var store = new InMemoryAuthTokenStore();
        var token = store.IssueToken(NewSession());

        var removed = store.RevokeToken(token);

        Assert.True(removed);
        Assert.False(store.TryGetSession(token, out _));
    }

    [Fact]
    public void RevokeToken_ReturnsFalseForUnknownToken()
    {
        var store = new InMemoryAuthTokenStore();

        Assert.False(store.RevokeToken("unknown"));
    }

    private static AuthenticatedUserSession NewSession()
    {
        return new AuthenticatedUserSession(
            new UserId(1),
            "Admin User",
            "admin@eventos.com",
            "administrador",
            new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero));
    }
}
