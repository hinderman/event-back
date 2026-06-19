using EventosVivos.Application.Abstractions.Messaging;
using EventosVivos.Application.Abstractions.Persistence;
using EventosVivos.Application.Abstractions.Clock;
using EventosVivos.Application.Common.Errors;
using EventosVivos.Domain.Catalogs;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Application.Reports;

public sealed class GetEventOccupancyReportQueryHandler(
    IReportingRepository reportingRepository,
    IEventRepository eventRepository,
    ICatalogRepository catalogRepository,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetEventOccupancyReportQuery, IReadOnlyCollection<EventOccupancyReportResponse>>
{
    public async Task<IReadOnlyCollection<EventOccupancyReportResponse>> Handle(
        GetEventOccupancyReportQuery request,
        CancellationToken cancellationToken)
    {
        var eventId = ToEventId(request.EventId);
        await CompletePastEventsAsync(cancellationToken);

        return await reportingRepository.GetEventOccupancyAsync(eventId, cancellationToken);
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

    private static EventId? ToEventId(long? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        if (value.Value <= 0)
        {
            throw new ApplicationRuleException("El evento debe ser mayor que cero.");
        }

        return new EventId(value.Value);
    }
}
