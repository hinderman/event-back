using EventosVivos.Domain.ValueObjects;
using EventosVivos.Domain.Venues;

namespace EventosVivos.Application.Abstractions.Persistence;

public interface IVenueRepository
{
    Task<Venue?> GetByIdAsync(VenueId venueId, CancellationToken cancellationToken = default);
}
