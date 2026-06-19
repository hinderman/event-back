using EventosVivos.Application.Reports;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Application.Abstractions.Persistence;

public interface IReportingRepository
{
    Task<IReadOnlyCollection<EventOccupancyReportResponse>> GetEventOccupancyAsync(
        EventId? eventId = null,
        CancellationToken cancellationToken = default);
}
