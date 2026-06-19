using EventosVivos.Application.Abstractions.Messaging;
using EventosVivos.Application.Abstractions.Persistence;
using EventosVivos.Application.Common.Errors;
using EventosVivos.Application.Events.ListEvents;

namespace EventosVivos.Application.Events.GetEventById.Handlers;

public sealed class GetEventByIdQueryHandler(IEventReadRepository eventReadRepository)
    : IQueryHandler<GetEventByIdQuery, EventSummaryResponse>
{
    public async Task<EventSummaryResponse> Handle(
        GetEventByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (request.EventId <= 0)
        {
            throw new ApplicationRuleException("El evento debe ser mayor que cero.");
        }

        return await eventReadRepository.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new NotFoundException("Evento", request.EventId);
    }
}
