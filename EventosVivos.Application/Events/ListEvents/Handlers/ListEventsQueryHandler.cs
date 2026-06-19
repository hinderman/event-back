using EventosVivos.Application.Abstractions.Messaging;
using EventosVivos.Application.Abstractions.Persistence;
using EventosVivos.Application.Abstractions.Clock;
using EventosVivos.Application.Common.Errors;
using EventosVivos.Application.Common.Pagination;
using EventosVivos.Domain.Catalogs;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Application.Events.ListEvents;

public sealed class ListEventsQueryHandler(
    IEventReadRepository eventReadRepository,
    IEventRepository eventRepository,
    ICatalogRepository catalogRepository,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<ListEventsQuery, PagedResponse<EventSummaryResponse>>
{
    public async Task<PagedResponse<EventSummaryResponse>> Handle(
        ListEventsQuery request,
        CancellationToken cancellationToken)
    {
        var criteria = new EventSearchCriteria(
            ToCatalogCode(request.EventTypeCode),
            request.StartsFrom,
            request.StartsTo,
            ToVenueId(request.VenueId),
            ToCatalogCode(request.EventStatusCode),
            NormalizeTitle(request.Title),
            NormalizePageNumber(request.PageNumber),
            NormalizePageSize(request.PageSize));

        EnsureDateRange(criteria.StartsFrom, criteria.StartsTo);
        await CompletePastEventsAsync(cancellationToken);

        return await eventReadRepository.SearchAsync(criteria, cancellationToken);
    }

    private async Task CompletePastEventsAsync(CancellationToken cancellationToken)
    {
        var activeStatus = await GetEventStatusAsync(EventStatus.ActiveCode, cancellationToken);
        var completedStatus = await GetEventStatusAsync(EventStatus.CompletedCode, cancellationToken);

        await eventRepository.CompletePastActiveEventsAsync(
            activeStatus.Id,
            completedStatus.Id,
            dateTimeProvider.UtcNow,
            cancellationToken);
    }

    private async Task<EventStatus> GetEventStatusAsync(string code, CancellationToken cancellationToken)
    {
        var statusCode = new CatalogCode(code);
        var status = await catalogRepository.GetEventStatusByCodeAsync(statusCode, cancellationToken);

        return status ?? throw new ApplicationRuleException($"El estado de evento '{code}' no esta configurado.");
    }

    private static void EnsureDateRange(DateOnly? startsFrom, DateOnly? startsTo)
    {
        if (startsFrom.HasValue && startsTo.HasValue && startsTo.Value < startsFrom.Value)
        {
            throw new ApplicationRuleException("La fecha final del filtro no puede ser anterior a la fecha inicial.");
        }
    }

    private static CatalogCode? ToCatalogCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : new CatalogCode(value);
    }

    private static VenueId? ToVenueId(int? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        if (value.Value <= 0)
        {
            throw new ApplicationRuleException("El venue debe ser mayor que cero.");
        }

        return new VenueId(value.Value);
    }

    private static string? NormalizeTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        title = title.Trim();

        if (title.Length > 100)
        {
            throw new ApplicationRuleException("La busqueda por titulo no puede superar 100 caracteres.");
        }

        return title;
    }

    private static int NormalizePageNumber(int pageNumber)
    {
        if (pageNumber <= 0)
        {
            throw new ApplicationRuleException("El numero de pagina debe ser mayor que cero.");
        }

        return pageNumber;
    }

    private static int NormalizePageSize(int pageSize)
    {
        if (pageSize is <= 0 or > 50)
        {
            throw new ApplicationRuleException("El tamano de pagina debe estar entre 1 y 50.");
        }

        return pageSize;
    }
}
