using EventosVivos.Application.Common.Errors;
using EventosVivos.Domain.Primitives;
using Microsoft.AspNetCore.Diagnostics;

namespace EventosVivos.Api.Infrastructure;

public static class ErrorHandlingExtensions
{
    public static WebApplication UseApiExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                var (statusCode, title, detail) = MapException(exception);

                await Results.Problem(
                        title: title,
                        detail: detail,
                        statusCode: statusCode)
                    .ExecuteAsync(context);
            });
        });

        return app;
    }

    private static (int StatusCode, string Title, string Detail) MapException(Exception? exception)
    {
        return exception switch
        {
            NotFoundException notFound => (
                StatusCodes.Status404NotFound,
                "Resource not found",
                notFound.Message),
            ForbiddenAccessException forbidden => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                forbidden.Message),
            AuthUnauthorizedException unauthorized => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                unauthorized.Message),
            ConflictException conflict => (
                StatusCodes.Status409Conflict,
                "Conflict",
                conflict.Message),
            ApplicationRuleException applicationRule => (
                StatusCodes.Status400BadRequest,
                "Application rule violation",
                applicationRule.Message),
            DomainException domain => (
                StatusCodes.Status400BadRequest,
                "Domain rule violation",
                domain.Message),
            BadHttpRequestException badRequest => (
                StatusCodes.Status400BadRequest,
                "Invalid request",
                badRequest.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Unexpected error",
                "Ocurrio un error inesperado procesando la solicitud.")
        };
    }
}
