using EventosVivos.Application.Abstractions.Persistence;
using EventosVivos.Domain.ValueObjects;
using EventosVivos.Domain.Venues;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class VenueRepository(EventosVivosDbContext dbContext) : IVenueRepository
{
    public Task<Venue?> GetByIdAsync(VenueId venueId, CancellationToken cancellationToken = default)
    {
        return dbContext.Venues
            .AsNoTracking()
            .SingleOrDefaultAsync(venue => venue.Id == venueId, cancellationToken);
    }
}
