using System.Security.Claims;

namespace EventosVivos.Api.Infrastructure;

public static class AuthenticatedUserExtensions
{
    public static long GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);

        return long.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("No fue posible identificar al usuario autenticado.");
    }
}
