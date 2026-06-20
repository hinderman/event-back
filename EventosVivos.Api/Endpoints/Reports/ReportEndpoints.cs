using EventosVivos.Api.Infrastructure;
using EventosVivos.Application.Reports;
using MediatR;

namespace EventosVivos.Api.Endpoints.Reports;

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
        }).RequireAuthorization(AuthPolicies.Administrator);

        return endpoints;
    }
}
