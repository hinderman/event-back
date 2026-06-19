using EventosVivos.Api.Endpoints;
using EventosVivos.Api.Infrastructure;
using EventosVivos.Application.Configuration;
using EventosVivos.Infrastructure.Configuration;

const string CorsPolicyName = "AngularClient";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
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

app.MapGet("/", () => Results.Redirect("/api/health"));
app.MapHealthEndpoints();
app.MapAuthEndpoints();
app.MapEventEndpoints();
app.MapReservationEndpoints();
app.MapReportEndpoints();

app.Run();
