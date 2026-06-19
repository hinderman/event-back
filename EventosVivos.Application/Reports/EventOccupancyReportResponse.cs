namespace EventosVivos.Application.Reports;

public sealed record EventOccupancyReportResponse(
    long EventId,
    string Title,
    string EventTypeCode,
    string EventStatusCode,
    int VenueId,
    string VenueName,
    string City,
    int MaxCapacity,
    int ReservedTickets,
    int ConfirmedTickets,
    int AvailableTickets,
    decimal OccupancyPercentage,
    decimal ConfirmedRevenue,
    decimal ProjectedRevenue);
