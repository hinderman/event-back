using EventosVivos.Application.Abstractions.Messaging;
using EventosVivos.Application.Events.ListEvents;

namespace EventosVivos.Application.Events.GetEventById;

public sealed record GetEventByIdQuery(long EventId) : IQuery<EventSummaryResponse>;
