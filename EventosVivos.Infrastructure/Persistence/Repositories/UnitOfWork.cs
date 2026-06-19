using EventosVivos.Application.Abstractions.Persistence;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class UnitOfWork(EventosVivosDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
