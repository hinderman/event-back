using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Application.Events.ListEvents;

public sealed record EventSearchCriteria(
    CatalogCode? EventTypeCode,
    DateOnly? StartsFrom,
    DateOnly? StartsTo,
    VenueId? VenueId,
    CatalogCode? EventStatusCode,
    string? Title,
    int PageNumber,
    int PageSize);
