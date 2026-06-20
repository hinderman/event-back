using EventosVivos.Api.Endpoints.Auth;
using EventosVivos.Api.Endpoints.Events;
using EventosVivos.Api.Endpoints.Health;
using EventosVivos.Api.Endpoints.Reports;
using EventosVivos.Api.Endpoints.Reservations;
using EventosVivos.Api.Infrastructure;
using EventosVivos.Application.Configuration;
using EventosVivos.Domain.Catalogs;
using EventosVivos.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication;

const string CorsPolicyName = "AngularClient";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddAuthentication(OpaqueTokenAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, OpaqueTokenAuthenticationHandler>(
        OpaqueTokenAuthenticationHandler.SchemeName,
        options => { });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthPolicies.Administrator, policy =>
        policy.RequireRole(UserType.AdministratorCode));
    options.AddPolicy(AuthPolicies.Buyer, policy =>
        policy.RequireRole(UserType.BuyerCode));
    options.AddPolicy(AuthPolicies.AdministratorOrBuyer, policy =>
        policy.RequireRole(UserType.AdministratorCode, UserType.BuyerCode));
});
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        policy.AllowAnyHeader()
            .AllowAnyMethod();

        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins);
            return;
        }

        policy.AllowAnyOrigin();
    });
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("EventosVivos"));

var app = builder.Build();

app.UseApiExceptionHandler();
app.UseCors(CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/api/health"));
app.MapHealthEndpoints();
app.MapAuthEndpoints();
app.MapEventEndpoints();
app.MapReservationEndpoints();
app.MapReportEndpoints();

app.Run();
