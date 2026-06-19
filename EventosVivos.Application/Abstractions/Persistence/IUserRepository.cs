using EventosVivos.Domain.Users;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<AppUser?> GetByIdAsync(UserId userId, CancellationToken cancellationToken = default);

    Task<AppUser?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task AddAsync(AppUser user, CancellationToken cancellationToken = default);
}
