using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Application.Abstractions.Auth;

public interface IPasswordHasher
{
    PasswordHash Hash(string password);

    bool Verify(string password, PasswordHash passwordHash);
}
