using EventosVivos.Application.Abstractions.Messaging;

namespace EventosVivos.Application.Reports;

public sealed record GetEventOccupancyReportQuery(long? EventId = null)
    : IQuery<IReadOnlyCollection<EventOccupancyReportResponse>>;
