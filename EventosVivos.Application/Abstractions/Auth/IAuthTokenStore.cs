using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Application.Abstractions.Auth;

public interface IAuthTokenStore
{
    string IssueToken(AuthenticatedUserSession session);

    bool RevokeToken(string token);
}

public sealed record AuthenticatedUserSession(
    UserId UserId,
    string FullName,
    string Email,
    string Role,
    DateTimeOffset IssuedAt);
