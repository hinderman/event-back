using EventosVivos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Api.Endpoints.Health;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/health")
            .WithTags("Health");

        group.MapGet("/", () => Results.Ok(new
        {
            status = "Healthy",
            checkedAt = DateTimeOffset.UtcNow
        }));

        group.MapGet("/database", async (
            EventosVivosDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var canConnect = await CanConnectAsync(dbContext, cancellationToken);

            return canConnect
                ? Results.Ok(new { status = "Healthy", database = "Reachable" })
                : Results.Problem(
                    title: "Database unavailable",
                    detail: "No fue posible conectar con la base de datos configurada.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
        });

        return endpoints;
    }

    private static async Task<bool> CanConnectAsync(
        EventosVivosDbContext dbContext,
        CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken);
        }
        catch
        {
            return false;
        }
    }
}
