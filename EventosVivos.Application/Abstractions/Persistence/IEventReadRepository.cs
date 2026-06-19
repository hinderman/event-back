using EventosVivos.Application.Events.ListEvents;
using EventosVivos.Application.Common.Pagination;

namespace EventosVivos.Application.Abstractions.Persistence;

public interface IEventReadRepository
{
    Task<PagedResponse<EventSummaryResponse>> SearchAsync(
        EventSearchCriteria criteria,
        CancellationToken cancellationToken = default);

    Task<EventSummaryResponse?> GetByIdAsync(
        long eventId,
        CancellationToken cancellationToken = default);
}
