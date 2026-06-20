using System.Collections.Concurrent;
using System.Security.Cryptography;
using EventosVivos.Application.Abstractions.Auth;

namespace EventosVivos.Infrastructure.Auth;

internal sealed class InMemoryAuthTokenStore : IAuthTokenStore
{
    private readonly ConcurrentDictionary<string, AuthenticatedUserSession> sessions = new();

    public string IssueToken(AuthenticatedUserSession session)
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = Base64UrlEncode(tokenBytes);

        sessions[token] = session;
        return token;
    }

    public bool TryGetSession(string token, out AuthenticatedUserSession session)
    {
        return sessions.TryGetValue(token, out session!);
    }

    public bool RevokeToken(string token)
    {
        return sessions.TryRemove(token, out _);
    }

    private static string Base64UrlEncode(byte[] value)
    {
        return Convert.ToBase64String(value)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
