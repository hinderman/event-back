using EventosVivos.Application.Reports;
using MediatR;

namespace EventosVivos.Api.Endpoints;

public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/reports")
            .WithTags("Reports");

        group.MapGet("/occupancy", async (
            ISender sender,
            long? eventId,
            CancellationToken cancellationToken) =>
        {
            var response = await sender.Send(new GetEventOccupancyReportQuery(eventId), cancellationToken);
            return Results.Ok(response);
        });

        return endpoints;
    }
}
