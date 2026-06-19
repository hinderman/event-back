using EventosVivos.Application.Abstractions.Auth;
using EventosVivos.Application.Abstractions.Messaging;
using EventosVivos.Application.Common.Errors;

namespace EventosVivos.Application.Auth.Logout;

public sealed class LogoutCommandHandler(IAuthTokenStore authTokenStore)
    : ICommandHandler<LogoutCommand, Unit>
{
    public Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || !authTokenStore.RevokeToken(request.Token))
        {
            throw new AuthUnauthorizedException("El token no es valido o ya fue cerrado.");
        }

        return Task.FromResult(Unit.Value);
    }
}
