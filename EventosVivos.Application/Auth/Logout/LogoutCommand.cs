using EventosVivos.Application.Abstractions.Messaging;

namespace EventosVivos.Application.Auth.Logout;

public sealed record LogoutCommand(string Token) : ICommand<Unit>;

public readonly record struct Unit
{
    public static readonly Unit Value = new();
}
