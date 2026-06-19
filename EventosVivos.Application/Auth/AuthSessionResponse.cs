using EventosVivos.Domain.Users;

namespace EventosVivos.Application.Auth;

public sealed record AuthSessionResponse(
    long UserId,
    string FullName,
    string Email,
    string Role,
    string Token)
{
    public static AuthSessionResponse Create(AppUser user, string role, string token)
    {
        return new AuthSessionResponse(
            user.Id.Value,
            user.FullName,
            user.Email.Value,
            role,
            token);
    }
}
