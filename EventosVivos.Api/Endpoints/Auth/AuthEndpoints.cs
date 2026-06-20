using EventosVivos.Application.Auth.Login;
using EventosVivos.Application.Auth.Logout;
using EventosVivos.Application.Auth.Register;
using EventosVivos.Api.Infrastructure;
using EventosVivos.Api.Endpoints.Auth.Requests;
using EventosVivos.Domain.Catalogs;
using MediatR;

namespace EventosVivos.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/register", async (
            RegisterAccountRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var response = await sender.Send(
                new RegisterAccountCommand(
                    request.FullName,
                    request.Email,
                    request.Password,
                    UserType.BuyerCode),
                cancellationToken);

            return Results.Created($"/api/users/{response.UserId}", response);
        });

        group.MapPost("/login", async (
            LoginRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var response = await sender.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
            return Results.Ok(response);
        });

        group.MapPost("/logout", async (
            HttpRequest httpRequest,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var token = ReadBearerToken(httpRequest);
            await sender.Send(new LogoutCommand(token), cancellationToken);

            return Results.NoContent();
        }).RequireAuthorization(AuthPolicies.AdministratorOrBuyer);

        return endpoints;
    }

    private static string ReadBearerToken(HttpRequest request)
    {
        const string bearerPrefix = "Bearer ";
        var authorization = request.Headers.Authorization.ToString();

        if (!authorization.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return authorization[bearerPrefix.Length..].Trim();
    }
}
