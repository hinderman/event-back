using EventosVivos.Application.Abstractions.Persistence;
using EventosVivos.Domain.Users;
using EventosVivos.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(EventosVivosDbContext dbContext) : IUserRepository
{
    public Task<AppUser?> GetByIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return dbContext.AppUsers
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);
    }

    public Task<AppUser?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return dbContext.AppUsers
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    public async Task AddAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        await dbContext.AppUsers.AddAsync(user, cancellationToken);
    }
}
