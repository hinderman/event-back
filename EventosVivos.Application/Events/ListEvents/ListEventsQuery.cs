using EventosVivos.Application.Abstractions.Messaging;
using EventosVivos.Application.Common.Pagination;

namespace EventosVivos.Application.Events.ListEvents;

public sealed record ListEventsQuery(
    string? EventTypeCode = null,
    DateOnly? StartsFrom = null,
    DateOnly? StartsTo = null,
    int? VenueId = null,
    string? EventStatusCode = null,
    string? Title = null,
    int PageNumber = 1,
    int PageSize = 10) : IQuery<PagedResponse<EventSummaryResponse>>;
