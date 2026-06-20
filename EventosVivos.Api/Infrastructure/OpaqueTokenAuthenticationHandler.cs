using System.Security.Claims;
using System.Text.Encodings.Web;
using EventosVivos.Application.Abstractions.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace EventosVivos.Api.Infrastructure;

public sealed class OpaqueTokenAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IAuthTokenStore tokenStore)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "OpaqueToken";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        const string bearerPrefix = "Bearer ";
        var authorization = Request.Headers.Authorization.ToString();

        if (!authorization.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var token = authorization[bearerPrefix.Length..].Trim();

        if (string.IsNullOrWhiteSpace(token) || !tokenStore.TryGetSession(token, out var session))
        {
            return Task.FromResult(AuthenticateResult.Fail("El token no es valido."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, session.UserId.Value.ToString()),
            new Claim(ClaimTypes.Name, session.FullName),
            new Claim(ClaimTypes.Email, session.Email),
            new Claim(ClaimTypes.Role, session.Role)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
